using System;
using System.Data.SqlClient;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Configuration;

namespace CricketApp.CompeteTracker
{
    public partial class Manage : System.Web.UI.Page
    {
        string connString = ConfigurationManager.ConnectionStrings["CricketDb"].ConnectionString;

        protected void Page_Load(object sender, EventArgs e)
        {
        }

        protected void btnAddMember_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(txtNewMember.Text))
            {
                using (SqlConnection con = new SqlConnection(connString))
                {
                    string query = "INSERT INTO Members (Name) VALUES (@Name)";
                    using (SqlCommand cmd = new SqlCommand(query, con))
                    {
                        cmd.Parameters.AddWithValue("@Name", txtNewMember.Text.Trim());
                        con.Open();
                        cmd.ExecuteNonQuery();
                    }
                }
                txtNewMember.Text = "";
                gridMembers.DataBind(); // Refresh grid
                gridQuestions.DataBind(); // Refresh questions grid in case dropdown needs the new member
            }
        }

        protected void btnAddQuestion_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(txtNewQuestion.Text))
            {
                using (SqlConnection con = new SqlConnection(connString))
                {
                    string query = "INSERT INTO Questions(QuestionTitle, Status, CreatedDate, ModifiedDate) VALUES(@Title, 'Pending', GETUTCDATE(), GETUTCDATE())";
                    
                    using (SqlCommand cmd = new SqlCommand(query, con))
                    {
                        cmd.Parameters.AddWithValue("@Title", txtNewQuestion.Text.Trim());
                        con.Open();
                        cmd.ExecuteNonQuery();
                    }
                }
                txtNewQuestion.Text = "";
                gridQuestions.DataBind(); // Refresh grid
            }
        }
    }
}