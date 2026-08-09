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
                SELECT ms.TeamScore, ms.Wickets, ms.Overs, ms.Balls, ms.Innings, ms.TargetScore,ms.LastUpdated,
                       t1.TeamName AS Team1Name, 
                       t2.TeamName AS Team2Name
                FROM MatchState ms
                INNER JOIN Matches m ON ms.MatchId = m.MatchId
                INNER JOIN Teams t1 ON m.Team1Id = t1.TeamId
                INNER JOIN Teams t2 ON m.Team2Id = t2.TeamId
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
                }
            }

            return currentData;
        }

        // 1. Create a new class to package the previous match data
        public class PreviousMatch
        {
            public string MatchTitle { get; set; }
            public string WinnerName { get; set; }
        }

        // 2. Add the new WebMethod to fetch completed matches
        [WebMethod]
        public static System.Collections.Generic.List<PreviousMatch> GetPreviousMatches()
        {
            var previousMatches = new System.Collections.Generic.List<PreviousMatch>();
            string connString = ConfigurationManager.ConnectionStrings["CricketDb"].ConnectionString;

            using (SqlConnection conn = new SqlConnection(connString))
            {
                // Query joins the Teams table 3 times to get the names for Team 1, Team 2, and the Winner
                string query = @"
                SELECT 
                    t1.TeamName AS Team1Name, 
                    t2.TeamName AS Team2Name, 
                    wt.TeamName AS WinnerName
                FROM Matches m
                INNER JOIN Teams t1 ON m.Team1Id = t1.TeamId
                INNER JOIN Teams t2 ON m.Team2Id = t2.TeamId
                INNER JOIN Teams wt ON m.WinnerTeamId = wt.TeamId
                WHERE m.IsLive = 0 AND m.WinnerTeamId IS NOT NULL
                ORDER BY m.MatchId DESC"; // Orders by newest completed match first

           SqlCommand cmd = new SqlCommand(query, conn);
                conn.Open();
                SqlDataReader reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    previousMatches.Add(new PreviousMatch
                    {
                        MatchTitle = reader["Team1Name"].ToString() + " vs " + reader["Team2Name"].ToString(),
                        WinnerName = reader["WinnerName"].ToString()
                    });
                }
            }
            return previousMatches;
        }

    }
}