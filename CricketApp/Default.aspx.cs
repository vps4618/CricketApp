using System;
using System.Configuration;
using System.Data.SqlClient;
using System.Web.Services;

namespace CricketApp
{
    // A simple class to package the data for JSON transfer
    public class MatchData
    {
        public string Team1Name { get; set; }
        public string Team2Name { get; set; }
        public int TeamScore { get; set; }
        public int Wickets { get; set; }
        public int Overs { get; set; }
        public int Balls { get; set; }
        public string LastUpdatedTime { get; set; } // New property for the UI
        public int Innings { get; set; }
        public int TargetScore { get; set; }
        public string TossWinnerName { get; set; }
    }

    public partial class Default : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
        }

        [WebMethod]
        public static MatchData GetLiveScore()
        {
            MatchData currentData = new MatchData();
            string connString = ConfigurationManager.ConnectionStrings["CricketDb"].ConnectionString;

            using (SqlConnection conn = new SqlConnection(connString))
            {
                // 2. Update the SQL query to join the Teams table twice (once for each team)
                string query = @"
SELECT ms.TeamScore, ms.Wickets, ms.Overs, ms.Balls, ms.Innings, ms.TargetScore, ms.LastUpdated,
       t1.TeamName AS Team1Name, 
       t2.TeamName AS Team2Name,
       tw.TeamName AS TossWinnerName
FROM MatchState ms
INNER JOIN Matches m ON ms.MatchId = m.MatchId
INNER JOIN Teams t1 ON m.Team1Id = t1.TeamId
INNER JOIN Teams t2 ON m.Team2Id = t2.TeamId
LEFT JOIN Teams tw ON m.TossWinnerId = tw.TeamId
WHERE m.IsLive = 1";
                // Read the single row from the database
                SqlCommand cmd = new SqlCommand(query, conn);
                conn.Open();
                SqlDataReader reader = cmd.ExecuteReader();

                if (reader.Read())
                {
                    // 3. Map the team names from the database to your object
                    currentData.Team1Name = reader["Team1Name"].ToString();
                    currentData.Team2Name = reader["Team2Name"].ToString();

                    currentData.TeamScore = Convert.ToInt32(reader["TeamScore"]);
                    currentData.Wickets = Convert.ToInt32(reader["Wickets"]);
                    currentData.Overs = Convert.ToInt32(reader["Overs"]);
                    currentData.Balls = Convert.ToInt32(reader["Balls"]);
                    currentData.Innings = Convert.ToInt32(reader["Innings"]);
                    currentData.TargetScore = Convert.ToInt32(reader["TargetScore"]);

                    // Format the database datetime into a friendly 12-hour clock (e.g., "11:45:30 AM")
                    currentData.LastUpdatedTime = Convert.ToDateTime(reader["LastUpdated"]).ToString("hh:mm:ss tt");
                    currentData.TossWinnerName = reader["TossWinnerName"].ToString();
                }
            }

            return currentData;
        }

        // 1. Updated class to hold batting details
        public class PreviousMatch
        {
            public string FirstBatName { get; set; }
            public string FirstBatScore { get; set; }
            public string SecondBatName { get; set; }
            public string SecondBatScore { get; set; }
            public string WinnerName { get; set; }
            public string TossWinnerName { get; set; }
        }

        // 2. Updated WebMethod to fetch and calculate the scores
        [WebMethod]
        public static System.Collections.Generic.List<PreviousMatch> GetPreviousMatches()
        {
            var previousMatches = new System.Collections.Generic.List<PreviousMatch>();
            string connString = ConfigurationManager.ConnectionStrings["CricketDb"].ConnectionString;

            using (SqlConnection conn = new SqlConnection(connString))
            {
                // Join MatchState to get the final score and innings data
                string query = @"
SELECT 
    t1.TeamName AS Team1Name, 
    t2.TeamName AS Team2Name, 
    wt.TeamName AS WinnerName,
    tw.TeamName AS TossWinnerName,
    ms.TeamScore, ms.Wickets, ms.Innings, ms.TargetScore
FROM Matches m
INNER JOIN Teams t1 ON m.Team1Id = t1.TeamId
INNER JOIN Teams t2 ON m.Team2Id = t2.TeamId
INNER JOIN Teams wt ON m.WinnerTeamId = wt.TeamId
LEFT JOIN Teams tw ON m.TossWinnerId = tw.TeamId
INNER JOIN MatchState ms ON m.MatchId = ms.MatchId
WHERE m.IsLive = 0 AND m.WinnerTeamId IS NOT NULL
ORDER BY m.MatchId DESC";

                SqlCommand cmd = new SqlCommand(query, conn);
                conn.Open();
                SqlDataReader reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    int innings = Convert.ToInt32(reader["Innings"]);
                    string t1Name = reader["Team1Name"].ToString();
                    string t2Name = reader["Team2Name"].ToString();
                    int currentScore = Convert.ToInt32(reader["TeamScore"]);
                    int currentWickets = Convert.ToInt32(reader["Wickets"]);
                    int targetScore = Convert.ToInt32(reader["TargetScore"]);

                    PreviousMatch match = new PreviousMatch();
                    match.WinnerName = reader["WinnerName"].ToString();

                    match.TossWinnerName = reader["TossWinnerName"].ToString();

                    if (innings == 2)
                    {
                        // Because teams are swapped at halftime, Team 2 batted first, Team 1 batted second.
                        match.FirstBatName = t2Name;

                        // The first team's score is the Target Score minus 1. 
                        // (Note: Exact wickets for the 1st innings aren't preserved in MatchState after the swap)
                        match.FirstBatScore = (targetScore - 1).ToString() + " runs";

                        match.SecondBatName = t1Name;
                        match.SecondBatScore = $"{currentScore}/{currentWickets}";
                    }
                    else
                    {
                        // Match ended during the 1st innings (e.g., rain or forfeit)
                        match.FirstBatName = t1Name;
                        match.FirstBatScore = $"{currentScore}/{currentWickets}";

                        match.SecondBatName = t2Name;
                        match.SecondBatScore = "Did not bat";
                    }

                    previousMatches.Add(match);
                }
            }
            return previousMatches;
        }

    }
}