using System;
using System.Configuration;
using System.Data.SqlClient;

namespace CricketApp
{
    public partial class ResetDatabase : System.Web.UI.Page
    {
        string connString = ConfigurationManager.ConnectionStrings["CricketDb"].ConnectionString;

        protected void Page_Load(object sender, EventArgs e)
        {
            // 1. STRICT PASSWORD PROTECTION SECURITY CHECK
            if (Session["IsAdmin"] == null || (bool)Session["IsAdmin"] == false)
            {
                // Kick to Admin login screen if not authenticated
                Response.Redirect("admin");
            }
        }

        protected void btnDeleteAll_Click(object sender, EventArgs e)
        {
            // The script drops tables in REVERSE dependency order.
            // You cannot drop Teams if Matches depends on it. 
            // You cannot drop Matches if MatchState, MatchLogs, or MatchStateHistory depend on it.
            string dropScript = @"
                IF OBJECT_ID('dbo.MatchLogs', 'U') IS NOT NULL 
                    DROP TABLE dbo.MatchLogs;

                IF OBJECT_ID('dbo.MatchStateHistory', 'U') IS NOT NULL 
                    DROP TABLE dbo.MatchStateHistory;

                IF OBJECT_ID('dbo.MatchState', 'U') IS NOT NULL 
                    DROP TABLE dbo.MatchState;

                IF OBJECT_ID('dbo.Matches', 'U') IS NOT NULL 
                    DROP TABLE dbo.Matches;

                IF OBJECT_ID('dbo.Teams', 'U') IS NOT NULL 
                    DROP TABLE dbo.Teams;





                -- CREEATING EMPTY TABLES    
                
                -- 1. Store all your tournament teams
                IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='Teams' AND xtype='U')
                BEGIN
                    CREATE TABLE Teams (
                        TeamId INT IDENTITY(1,1) PRIMARY KEY,
                        TeamName NVARCHAR(100)
                    );
                END

                -- 2. Store the tournament matches and track the winner
                IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='Matches' AND xtype='U')
                BEGIN
                    CREATE TABLE Matches (
                        MatchId INT IDENTITY(1,1) PRIMARY KEY,
                        Team1Id INT FOREIGN KEY REFERENCES Teams(TeamId),
                        Team2Id INT FOREIGN KEY REFERENCES Teams(TeamId),
                        WinnerTeamId INT NULL,
                        IsLive BIT DEFAULT 0,
                        TossWinnerId INT NULL
                    );
                END

                -- 3. The live scoreboard
                IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='MatchState' AND xtype='U')
                BEGIN
                    CREATE TABLE MatchState (
                        MatchId INT PRIMARY KEY FOREIGN KEY REFERENCES Matches(MatchId),
                        BattingTeamId INT FOREIGN KEY REFERENCES Teams(TeamId),
                        TeamScore INT,
                        Wickets INT,
                        Overs INT,
                        Balls INT,
                        LastUpdated DATETIME,
                        Innings INT DEFAULT 1,
                        TargetScore INT DEFAULT 0
                    );
                END

                -- 4. sql query for creating logs
                IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='MatchLogs' AND xtype='U')
                BEGIN
                    CREATE TABLE MatchLogs (
                        LogId INT IDENTITY(1,1) PRIMARY KEY,
                        MatchId INT FOREIGN KEY REFERENCES Matches(MatchId),
                        LogMessage NVARCHAR(500),
                        LogTime DATETIME DEFAULT GETDATE()
                    );
                END

                -- 5. Match state history
                IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='MatchStateHistory' AND xtype='U')
                BEGIN
                    CREATE TABLE MatchStateHistory (
                        HistoryId INT IDENTITY(1,1) PRIMARY KEY,
                        MatchId INT FOREIGN KEY REFERENCES Matches(MatchId),
                        TeamScore INT,
                        Wickets INT,
                        Overs INT,
                        Balls INT,
                        Innings INT,
                        TargetScore INT
                    );
                END

                -- 6. Insert Initial Rows IF the Teams table is completely empty
                IF NOT EXISTS (SELECT 1 FROM Teams)
                BEGIN
                    INSERT INTO Teams (TeamName) VALUES ('Ingiriya Strikers');
                    INSERT INTO Teams (TeamName) VALUES ('Moratuwa Kings');
                END
            ";

            try
            {
                using (SqlConnection conn = new SqlConnection(connString))
                {
                    using (SqlCommand cmd = new SqlCommand(dropScript, conn))
                    {
                        conn.Open();
                        cmd.ExecuteNonQuery();
                    }
                }

                lblMessage.Text = "SUCCESS: All tables have been permanently deleted. Empty tables created.";
                lblMessage.ForeColor = System.Drawing.Color.Green;

                // Disable the button to prevent double-clicking
                btnDeleteAll.Enabled = false;
            }
            catch (Exception ex)
            {
                lblMessage.Text = "CRITICAL ERROR: " + ex.Message;
                lblMessage.ForeColor = System.Drawing.Color.DarkRed;
            }
        }
    }
}