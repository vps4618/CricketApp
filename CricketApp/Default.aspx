<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Default.aspx.cs" Inherits="CricketApp.Default" %>

<!DOCTYPE html>
<html>
<head runat="server">
    <title>Live Match</title>
    <!-- The viewport meta tag is strictly required for mobile scaling -->
    <meta name="viewport" content="width=device-width, initial-scale=1.0, maximum-scale=1.0, user-scalable=no">
    <style>
        /* Global Reset: This fixes the overflowing box bug shown in your image */
        * {
            box-sizing: border-box;
        }

        body {
            font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif;
            background-color: #f4f4f9;
            text-align: center;
            margin: 0;
            padding: 20px 15px; /* Adds breathing room around the edges on mobile */
        }

        /* Universal Card Styling */
        .card {
            background: white;
            padding: 25px 20px;
            border-radius: 12px;
            box-shadow: 0 4px 10px rgba(0,0,0,0.08);
            width: 100%;
            max-width: 450px;
            margin: 0 auto 20px auto; /* Centers the card and adds bottom spacing */
        }

        /* Scoreboard Specifics */
        .live-indicator {
            color: #f44336;
            font-weight: bold;
            font-size: 13px;
            letter-spacing: 1px;
            animation: blinker 2s linear infinite;
            margin-bottom: 10px;
        }

        #matchTitle {
            color: #1976D2;
            margin-top: 0;
            margin-bottom: 15px;
            font-size: 22px;
            line-height: 1.3;
        }

        .score {
            font-size: 55px;
            font-weight: bold;
            color: #222;
            margin: 10px 0;
        }

        .overs {
            font-size: 20px;
            color: #555;
        }

        #targetContainer {
            font-size: 18px;
            color: #d32f2f;
            font-weight: bold;
            margin-top: 15px;
        }

        .last-updated {
            margin-top: 20px;
            font-size: 13px;
            color: #999;
        }

        /* Tournament Results Specifics */
        .results-header {
            color: #333;
            border-bottom: 2px solid #eee;
            padding-bottom: 10px;
            margin-top: 0;
            text-align: left;
            font-size: 18px;
        }

        #previousMatchesList {
            list-style-type: none;
            padding: 0;
            margin: 0;
            max-height: 300px;
            overflow-y: auto;
            text-align: left;
        }

            #previousMatchesList li {
                padding: 15px 0;
                border-bottom: 1px solid #eee;
            }

        .match-title {
            font-size: 15px;
            font-weight: 600;
            color: #424242;
        }

        .match-winner {
            font-size: 14px;
            color: #4CAF50;
            margin-top: 5px;
        }

        @keyframes blinker {
            50% {
                opacity: 0;
            }
        }

        /* Media Queries for very small screens (like older iPhones) */
        @media (max-width: 380px) {
            .score {
                font-size: 45px;
            }

            #matchTitle {
                font-size: 18px;
            }

            .card {
                padding: 20px 15px;
            }
        }
    </style>
</head>
<body>

    <!-- 1. LIVE SCORE CARD -->
    <div class="card">
        <div class="live-indicator">● LIVE</div>

        <h2 id="matchTitle">Match Offline</h2>

        <div class="score">
            <span id="score">0</span> / <span id="wickets">0</span>
        </div>

        <div class="overs">
            Overs: <span id="overs">0</span>.<span id="balls">0</span>
        </div>

        <div id="targetContainer" style="display: none;">
            Target: <span id="targetScore">0</span>
        </div>

        <div class="last-updated">
            Last Updated: <span id="lastUpdatedTime">--:--:--</span>
        </div>
    </div>

    <!-- 2. TOURNAMENT RESULTS CARD -->
    <div class="card">
        <h3 class="results-header">Tournament Results</h3>
        <ul id="previousMatchesList">
            <li style="color: #888; font-style: italic;">Loading past results...</li>
        </ul>
    </div>

    <script>
        // --- 1. LIVE SCORE FETCH LOGIC ---
        async function fetchLiveScore() {
            try {
                const response = await fetch('Default.aspx/GetLiveScore', {
                    method: 'POST',
                    headers: { 'Content-Type': 'application/json' }
                });

                const data = await response.json();
                const matchState = data.d;

                if (matchState.Team1Name && matchState.Team2Name) {
                    let titleHtml = matchState.Team1Name + " vs " + matchState.Team2Name;
                    titleHtml += `<br><span style="font-size: 14px; color: #555;">🪙 Toss Winner: ${matchState.TossWinnerName}</span>`;
                    document.getElementById("matchTitle").innerHTML = titleHtml;
                } else {
                    document.getElementById("matchTitle").innerText = "Match Offline";
                }

                document.getElementById("score").innerText = matchState.TeamScore;
                document.getElementById("wickets").innerText = matchState.Wickets;
                document.getElementById("overs").innerText = matchState.Overs;
                document.getElementById("balls").innerText = matchState.Balls;
                document.getElementById("lastUpdatedTime").innerText = matchState.LastUpdatedTime;

                if (matchState.Innings === 2) {
                    document.getElementById("targetContainer").style.display = "block";
                    document.getElementById("targetScore").innerText = matchState.TargetScore;
                } else {
                    document.getElementById("targetContainer").style.display = "none";
                }

            } catch (error) {
                console.error("Error fetching score:", error);
            }
        }

        // --- 2. PREVIOUS MATCHES FETCH LOGIC ---
        async function fetchPreviousMatches() {
            try {
                const response = await fetch('Default.aspx/GetPreviousMatches', {
                    method: 'POST',
                    headers: { 'Content-Type': 'application/json' }
                });

                const data = await response.json();
                const matches = data.d;

                const listContainer = document.getElementById("previousMatchesList");
                listContainer.innerHTML = "";

                if (matches.length === 0) {
                    listContainer.innerHTML = "<li style='color: #888;'>No matches have been completed yet.</li>";
                    return;
                }

                matches.forEach(match => {
                    const listItem = document.createElement("li");

                    // Display First Bat, Second Bat, and the Winner clearly
                    listItem.innerHTML = `
    <div class="match-title" style="color: #1976D2; font-size: 16px; margin-bottom: 5px;">
        ${match.FirstBatName} vs ${match.SecondBatName}
    </div>
    <div style="font-size: 13px; color: #666; margin-bottom: 10px;">
        🪙 Toss Winner: <b>${match.TossWinnerName}</b>
    </div>
    <div style="font-size: 14px; color: #424242; margin-bottom: 4px;">
        🏏 <b>1st Bat:</b> ${match.FirstBatName} - <span style="color:#d32f2f; font-weight:bold;">${match.FirstBatScore}</span>
    </div>
    <div style="font-size: 14px; color: #424242; margin-bottom: 10px;">
        🏏 <b>2nd Bat:</b> ${match.SecondBatName} - <span style="color:#d32f2f; font-weight:bold;">${match.SecondBatScore}</span>
    </div>
    <div class="match-winner">🏆 Winner: ${match.WinnerName}</div>
`;
                    listContainer.appendChild(listItem);
                });

            } catch (error) {
                console.error("Error fetching previous matches:", error);
            }
        }

        // --- 3. TIMERS & INITIALIZATION ---
        fetchLiveScore();
        setInterval(fetchLiveScore, 3000); // 3 seconds

        fetchPreviousMatches();
        setInterval(fetchPreviousMatches, 12000); // 12 seconds
    </script>
</body>
</html>
