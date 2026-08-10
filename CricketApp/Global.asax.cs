using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Optimization;
using System.Web.Routing;
using System.Web.Security;
using System.Web.SessionState;
using System.Configuration;
using System.Data.SqlClient;

namespace CricketApp
{
    public class Global : HttpApplication
    {
        void Application_Start(object sender, EventArgs e)
        {
            // 1. Run database initialization
            InitializeDatabase();

            // Register your clean URL routes here
            RegisterRoutes(RouteTable.Routes);
        }

        static void RegisterRoutes(RouteCollection routes)
        {
            // MapPageRoute(RouteName, URL Pattern, Physical File)

            // Matches /default or /default/
            routes.MapPageRoute("DefaultRoute", "default", "~/Default.aspx");

            // Matches /admin or /admin/
            routes.MapPageRoute("AdminRoute", "admin", "~/Admin.aspx");

            // Matches /manageteams or /manageteams/
            routes.MapPageRoute("ManageTeamsRoute", "manageteams", "~/ManageTeams.aspx");

            // NEW: Matches /managematches or /managematches/
            routes.MapPageRoute("ManageMatchesRoute", "managematches", "~/ManageMatches.aspx");

            routes.MapPageRoute("ManageLogsRoute", "managelogs", "~/ManageLogs.aspx");

            // NEW: Matches /managematchstate or /managematchstate/
            routes.MapPageRoute("ManageMatchStateRoute", "managematchstate", "~/ManageMatchState.aspx");

            // NEW: Matches /managehistory or /managehistory/
            routes.MapPageRoute("ManageHistoryRoute", "managehistory", "~/ManageMatchStateHistory.aspx");

            // NEW: Public route directory
            routes.MapPageRoute("RoutingsRoute", "routes", "~/Routings.aspx");
            // NEW: Matches /resetdatabase or /resetdatabase/
            routes.MapPageRoute("ResetDatabaseRoute", "resetdatabase", "~/ResetDatabase.aspx");
        }

        private void InitializeDatabase()
        {
            string connString = ConfigurationManager.ConnectionStrings["CricketDb"].ConnectionString;

            // This SQL script safely checks if tables exist before creating them.
            // It also checks if the Teams table is empty before inserting the default rows.
            string setupScript = @"
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
                    using (SqlCommand cmd = new SqlCommand(setupScript, conn))
                    {
                        conn.Open();
                        cmd.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                // In a production app, you would log this error.
                // System.Diagnostics.Debug.WriteLine("Database initialization failed: " + ex.Message);
            }
        }
    }
}