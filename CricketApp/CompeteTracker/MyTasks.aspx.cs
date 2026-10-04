using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace CricketApp.CompeteTracker
{
    public partial class MyTasks : System.Web.UI.Page
    {
        string connString = ConfigurationManager.ConnectionStrings["CricketDb"].ConnectionString;

        protected void Page_Load(object sender, EventArgs e)
        {
            // Enforce authentication
            if (Session["MemberId"] == null)
            {
                Response.Redirect("Login.aspx");
            }

            if (!IsPostBack)
            {
                lblMemberName.Text = Session["MemberName"].ToString();
            }
        }
        protected void btnAddMyQuestion_Click(object sender, EventArgs e)
        {
            string questionTitle = txtMyNewQuestion.Text.Trim();

            // Reset the message label
            lblAddMessage.Text = "";

            if (!string.IsNullOrWhiteSpace(questionTitle))
            {
                using (SqlConnection con = new SqlConnection(connString))
                {
                    con.Open();

                    // 1. Check if the question exists and retrieve its assigned member
                    string checkQuery = "SELECT AssignedMemberId FROM Questions WHERE QuestionTitle = @Title";
                    using (SqlCommand checkCmd = new SqlCommand(checkQuery, con))
                    {
                        checkCmd.Parameters.AddWithValue("@Title", questionTitle);

                        // ExecuteScalar returns null if no record is found, or the value of the first column if found.
                        object result = checkCmd.ExecuteScalar();

                        if (result != null) // The question title already exists in the database
                        {
                            if (result != DBNull.Value)
                            {
                                // Someone is already assigned to it
                                lblAddMessage.Text = "Stop: This question has already been taken by someone!";
                                lblAddMessage.CssClass = "text-danger"; // Bootstrap red text
                                return; // Abort the rest of the method
                            }
                            else
                            {
                                // It exists, but no one is assigned. Claim it via UPDATE.
                                string updateQuery = "UPDATE Questions SET AssignedMemberId = @MemberId, Status = 'Ongoing' WHERE QuestionTitle = @Title";
                                using (SqlCommand updateCmd = new SqlCommand(updateQuery, con))
                                {
                                    updateCmd.Parameters.AddWithValue("@Title", questionTitle);
                                    updateCmd.Parameters.AddWithValue("@MemberId", Convert.ToInt32(Session["MemberId"]));
                                    updateCmd.ExecuteNonQuery();
                                }
                                lblAddMessage.Text = "Existing question successfully claimed!";
                            }
                        }
                        else
                        {
                            // 2. The question does not exist at all. Proceed with INSERT.
                            string insertQuery = "INSERT INTO Questions (QuestionTitle, Status, AssignedMemberId) VALUES (@Title, 'Ongoing', @MemberId)";
                            using (SqlCommand insertCmd = new SqlCommand(insertQuery, con))
                            {
                                insertCmd.Parameters.AddWithValue("@Title", questionTitle);
                                insertCmd.Parameters.AddWithValue("@MemberId", Convert.ToInt32(Session["MemberId"]));
                                insertCmd.ExecuteNonQuery();
                            }
                            lblAddMessage.Text = "New question added and successfully claimed!";
                        }
                    }
                }

                // 3. Clear the textbox and show success styling
                txtMyNewQuestion.Text = "";
                lblAddMessage.CssClass = "text-success"; // Bootstrap green text

                // 4. Refresh the grid to show the newly claimed question
                gridMyQuestions.DataBind();
            }
        }
        // Optional helper method to color-code statuses in the GridView (same as Default.aspx)
        protected string GetStatusColor(string status)
        {
            switch (status.ToLower())
            {
                case "ongoing": return "warning";
                case "finished": return "success";
                default: return "secondary";
            }
        }
    }
}