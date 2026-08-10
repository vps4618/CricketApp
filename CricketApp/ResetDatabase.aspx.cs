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

                lblMessage.Text = "SUCCESS: All tables have been permanently deleted.";
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