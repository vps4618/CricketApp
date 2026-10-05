using System;
using System.Configuration;
using System.Data.SqlClient;

namespace CricketApp.CompeteTracker
{
    public partial class ResetDatabase : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
        }

        protected void btnResetDatabase_Click(object sender, EventArgs e)
        {
            try
            {
                string connString = ConfigurationManager.ConnectionStrings["CricketDb"].ConnectionString;

                using (SqlConnection con = new SqlConnection(connString))
                {
                    con.Open();

                    // Drop existing tables if they exist
                    string dropTablesSQL = @"
                        IF EXISTS (SELECT * FROM sysobjects WHERE name='Questions' AND xtype='U')
                            DROP TABLE Questions;
                        
                        IF EXISTS (SELECT * FROM sysobjects WHERE name='Members' AND xtype='U')
                            DROP TABLE Members;
                    ";

                    using (SqlCommand cmd = new SqlCommand(dropTablesSQL, con))
                    {
                        cmd.ExecuteNonQuery();
                    }

                    // Create Members table
                    string createMembersTable = @"
                        CREATE TABLE Members (
                            MemberId INT IDENTITY(1,1) PRIMARY KEY,
                            Name NVARCHAR(100) NOT NULL
                        );
                    ";

                    using (SqlCommand cmd = new SqlCommand(createMembersTable, con))
                    {
                        cmd.ExecuteNonQuery();
                    }

                    // Create Questions table
                    string createQuestionsTable = @"
                        CREATE TABLE Questions (
                            QuestionId INT IDENTITY(1,1) PRIMARY KEY,
                            QuestionTitle NVARCHAR(255) NOT NULL UNIQUE,
                            Status NVARCHAR(50) DEFAULT 'Pending',
                            AssignedMemberId INT NULL,
                            CreatedDate DATETIME DEFAULT GETUTCDATE(),
                            ModifiedDate DATETIME NULL
                        );
                    ";

                    using (SqlCommand cmd = new SqlCommand(createQuestionsTable, con))
                    {
                        cmd.ExecuteNonQuery();
                    }
                }

                lblMessage.Text = "✓ Database reset successfully! All tables have been recreated.";
                lblMessage.CssClass = "alert alert-success d-block mb-3 font-weight-bold";
            }
            catch (Exception ex)
            {
                lblMessage.Text = "✗ Error: " + ex.Message;
                lblMessage.CssClass = "alert alert-danger d-block mb-3 font-weight-bold";
            }
        }
    }
}