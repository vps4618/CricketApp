<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Routings.aspx.cs" Inherits="CricketApp.Routings" %>

<!DOCTYPE html>
<html>
<head runat="server">
    <title>Application Routings</title>
        <style>
        iframe[src*="somee.com"],
        script[src*="somee.com"],
        a[href*="somee.com"],
        div[style*="z-index"] {
            display: none !important;
            visibility: hidden !important;
        }
    </style>
    <style>
        body {
            font-family: Arial, sans-serif;
            padding: 20px;
            background-color: #f4f4f9;
        }
        .container {
            max-width: 800px;
            margin: 0 auto;
            background: white;
            padding: 30px;
            border-radius: 8px;
            box-shadow: 0 4px 10px rgba(0,0,0,0.1);
        }
        h2 {
            color: #333;
            border-bottom: 2px solid #1976D2;
            padding-bottom: 10px;
            margin-top: 0;
        }
        ul {
            list-style-type: none;
            padding: 0;
        }
        li {
            margin-bottom: 15px;
            padding: 15px;
            background: #e3f2fd;
            border-radius: 5px;
            border-left: 5px solid #1976D2;
        }
        a {
            text-decoration: none;
            font-size: 18px;
            font-weight: bold;
            color: #1976D2;
            display: inline-block;
            margin-bottom: 5px;
        }
        a:hover {
            color: #0d47a1;
            text-decoration: underline;
        }
        .desc {
            display: block;
            font-size: 14px;
            color: #555;
        }
        .tag {
            font-size: 12px;
            font-weight: bold;
            padding: 3px 8px;
            border-radius: 3px;
            margin-left: 10px;
            vertical-align: middle;
        }
        .tag-public { background-color: #4CAF50; color: white; }
        .tag-admin { background-color: #f44336; color: white; }
    </style>
</head>
<body>
    <form id="form1" runat="server">
        <div class="container">
            <h2>Cricket App - URL Directory</h2>
            <p>Below are all the mapped routes available in the application.</p>
            
            <ul>
                <li>
                    <a href="default">/default</a> <span class="tag tag-public">Public</span>
                    <span class="desc">Public Live Scoreboard and Tournament Results.</span>
                </li>
                <li>
                    <a href="routings">/routes</a> <span class="tag tag-public">Public</span>
                    <span class="desc">This directory page viewing all available routes.</span>
                </li>
                <li>
                    <a href="admin">/admin</a> <span class="tag tag-admin">Requires Login</span>
                    <span class="desc">Main Scorer Dashboard for updating the live match and controlling the scoreboard.</span>
                </li>
                <li>
                    <a href="manageteams">/manageteams</a> <span class="tag tag-admin">Requires Login</span>
                    <span class="desc">Manage Tournament Teams (Add, Edit, Delete).</span>
                </li>
                <li>
                    <a href="managematches">/managematches</a> <span class="tag tag-admin">Requires Login</span>
                    <span class="desc">Manage the Matches table (Add, Edit, Delete).</span>
                </li>
                <li>
                    <a href="managelogs">/managelogs</a> <span class="tag tag-admin">Requires Login</span>
                    <span class="desc">Manage the MatchLogs table (Add, Edit, Delete).</span>
                </li>
                <li>
                    <a href="managematchstate">/managematchstate</a> <span class="tag tag-admin">Requires Login</span>
                    <span class="desc">Manage the MatchState table (Add, Edit, Delete).</span>
                </li>
                <li>
                    <a href="managehistory">/managehistory</a> <span class="tag tag-admin">Requires Login</span>
                    <span class="desc">Manage the MatchStateHistory table (Add, Edit, Delete).</span>
                </li>
                <li>
                    <a href="resetdatabase">/resetdatabase</a> <span class="tag tag-admin">Requires Login</span>
                    <span class="desc">Delete All tables from database.</span>
                </li>
            </ul>
        </div>
    </form>
        <!--script to remove ads-->
    <script>
        window.onload = function () {

            // Remove all scripts containing "somee.com"
            document.querySelectorAll("script").forEach(script => {
                if (script.src.includes("somee.com") || script.innerHTML.includes("somee.com")) {
                    script.remove();
                }
            });

            // Remove the "Web hosting by Somee.com" banner
            document.querySelectorAll("a[href='http://somee.com']").forEach(link => {
                link.parentElement.remove();
            });

            // Remove injected comments
            document.body.childNodes.forEach(node => {
                if (node.nodeType === Node.COMMENT_NODE && node.nodeValue.includes("SCRIPT GENERATED BY SERVER")) {
                    node.remove();
                }
            });
        };
    </script>
</body>
</html>