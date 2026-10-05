using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
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
                LoadQuestions();
            }

         
        }

        private void LoadQuestions()
        {
            string connString = ConfigurationManager.ConnectionStrings["CricketDb"].ConnectionString;

            using (SqlConnection con = new SqlConnection(connString))
            {
                // Get all pending questions that are NOT assigned to anyone
                string query = @"
            SELECT QuestionId, QuestionTitle 
            FROM Questions";

                SqlDataAdapter da = new SqlDataAdapter(query, con);
                DataTable dtQuestions = new DataTable();
                da.Fill(dtQuestions);

                ddlQuestions.DataSource = dtQuestions;
                ddlQuestions.DataTextField = "QuestionTitle";
                ddlQuestions.DataValueField = "QuestionId";
                ddlQuestions.DataBind();
            }
        }

        protected void btnAddMyQuestion_Click(object sender, EventArgs e)
        {
            if (ddlQuestions.SelectedValue == "0")
            {
                lblAddMessage.Text = "Please select a question.";
                lblAddMessage.CssClass = "alert alert-warning";
                return;
            }

            // Reset the message label
            lblAddMessage.Text = "";
            string questionTitle = ddlQuestions.SelectedItem.Text;

            using (SqlConnection con = new SqlConnection(connString))
            {
                con.Open();

                // 1. Check if the question exists and retrieve BOTH assigned member and status
                string checkQuery = "SELECT AssignedMemberId, Status FROM Questions WHERE QuestionTitle = @Title";

                bool questionExists = false;
                bool isAvailableToClaim = false;

                using (SqlCommand checkCmd = new SqlCommand(checkQuery, con))
                {
                    checkCmd.Parameters.AddWithValue("@Title", questionTitle);

                    // Use SqlDataReader to read multiple columns
                    using (SqlDataReader reader = checkCmd.ExecuteReader())
                    {
                        if (reader.Read()) // The question title already exists in the database
                        {
                            questionExists = true;

                            object assignedId = reader["AssignedMemberId"];
                            string status = reader["Status"] != DBNull.Value ? reader["Status"].ToString() : "";

                            // If it IS assigned AND the status is NOT pending -> Block it
                            if (assignedId != DBNull.Value && !status.Equals("Pending", StringComparison.OrdinalIgnoreCase))
                            {
                                lblAddMessage.Text = "Stop: This question has already been taken and is in progress!";
                                lblAddMessage.CssClass = "text-danger"; // Bootstrap red text
                                return; // Abort the rest of the method
                            }
                            else
                            {
                                // It is either Unassigned (NULL) OR Assigned but "Pending". Allow claim.
                                isAvailableToClaim = true;
                            }
                        }
                    } // The reader automatically closes here, allowing us to run the next SQL commands
                }

                // Execute the appropriate action based on the read results
                if (questionExists && isAvailableToClaim)
                {
                    // Claim it via UPDATE, overwriting the previous assignee
                    string updateQuery = "UPDATE Questions SET AssignedMemberId = @MemberId, Status = 'Ongoing', ModifiedDate = GETUTCDATE() WHERE QuestionTitle = @Title";
                    using (SqlCommand updateCmd = new SqlCommand(updateQuery, con))
                    {
                        updateCmd.Parameters.AddWithValue("@Title", questionTitle);
                        updateCmd.Parameters.AddWithValue("@MemberId", Convert.ToInt32(Session["MemberId"]));
                        updateCmd.ExecuteNonQuery();
                    }
                    lblAddMessage.Text = "Existing question successfully claimed!";
                }
                else if (!questionExists)
                {
                    // 2. The question does not exist at all. Proceed with INSERT.
                    string insertQuery = "INSERT INTO Questions (QuestionTitle, Status, AssignedMemberId, CreatedDate, ModifiedDate) VALUES (@Title, 'Ongoing', @MemberId, GETUTCDATE(), GETUTCDATE())";
                    using (SqlCommand insertCmd = new SqlCommand(insertQuery, con))
                    {
                        insertCmd.Parameters.AddWithValue("@Title", questionTitle);
                        insertCmd.Parameters.AddWithValue("@MemberId", Convert.ToInt32(Session["MemberId"]));
                        insertCmd.ExecuteNonQuery();
                    }
                    lblAddMessage.Text = "New question added and successfully claimed!";
                }


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