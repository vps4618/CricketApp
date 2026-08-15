using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Web.UI.WebControls;

namespace CricketApp
{
    public partial class ManageLogs : System.Web.UI.Page
    {
        string connString = ConfigurationManager.ConnectionStrings["CricketDb"].ConnectionString;

        protected void Page_Load(object sender, EventArgs e)
        {
            // 1. PASSWORD PROTECTION SECURITY CHECK
            if (Session["IsAdmin"] == null || (bool)Session["IsAdmin"] == false)
            {
                // If not authenticated, kick them back to the Admin login screen
                Response.Redirect("admin");
            }

            if (!IsPostBack)
            {
                LoadMatches();
                BindGrid();
            }
        }

        // --- Fetch matches for the Add Log DropDown ---
        private void LoadMatches()
        {
            using (SqlConnection conn = new SqlConnection(connString))
            {
                // Order by MatchId descending so the most recent matches are at the top
                SqlDataAdapter da = new SqlDataAdapter("SELECT MatchId FROM Matches ORDER BY MatchId DESC", conn);
                DataTable dt = new DataTable();
                da.Fill(dt);

                ddlMatchId.DataSource = dt;
                ddlMatchId.DataBind();
            }
        }

        // --- READ: Fetch data into GridView ---
        private void BindGrid()
        {
            using (SqlConnection conn = new SqlConnection(connString))
            {
                // Fetching all logs according to the schema
                string query = "SELECT LogId, MatchId, LogMessage, LogTime FROM MatchLogs ORDER BY LogId DESC";
                SqlDataAdapter da = new SqlDataAdapter(query, conn);
                DataTable dt = new DataTable();
                da.Fill(dt);

                gvLogs.DataSource = dt;
                gvLogs.DataBind();
            }
        }

        // --- CREATE: Add a new Log ---
        protected void btnAddLog_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtLogMessage.Text))
            {
                lblMessage.Text = "Log message cannot be empty.";
                lblMessage.ForeColor = System.Drawing.Color.Red;
                return;
            }

            if (ddlMatchId.SelectedValue == "")
            {
                lblMessage.Text = "You must select a Match ID.";
                lblMessage.ForeColor = System.Drawing.Color.Red;
                return;
            }

            using (SqlConnection conn = new SqlConnection(connString))
            {
                // Note: LogTime defaults to DATEADD(minute, 330, GETUTCDATE()) based on your schema
                string query = @"INSERT INTO MatchLogs (MatchId, LogMessage, LogTime) 
                                 VALUES (@MatchId, @LogMessage, DATEADD(minute, 330, GETUTCDATE()))";
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@MatchId", ddlMatchId.SelectedValue);
                cmd.Parameters.AddWithValue("@LogMessage", txtLogMessage.Text.Trim());

                conn.Open();
                cmd.ExecuteNonQuery();
            }

            txtLogMessage.Text = "";
            lblMessage.Text = "Log entry added successfully!";
            lblMessage.ForeColor = System.Drawing.Color.Green;
            BindGrid();
        }

        // --- UPDATE: Trigger Edit Mode ---
        protected void gvLogs_RowEditing(object sender, GridViewEditEventArgs e)
        {
            gvLogs.EditIndex = e.NewEditIndex;
            BindGrid();
        }

        // --- UPDATE: Cancel Edit Mode ---
        protected void gvLogs_RowCancelingEdit(object sender, GridViewCancelEditEventArgs e)
        {
            gvLogs.EditIndex = -1;
            BindGrid();
        }

        // --- UPDATE: Save Changes ---
        protected void gvLogs_RowUpdating(object sender, GridViewUpdateEventArgs e)
        {
            int logId = Convert.ToInt32(gvLogs.DataKeys[e.RowIndex].Value);
            GridViewRow row = gvLogs.Rows[e.RowIndex];

            // Accessing the BoundFields natively rendered by ASP.NET in Edit Mode
            string matchIdStr = ((TextBox)row.Cells[1].Controls[0]).Text;
            string logMessage = ((TextBox)row.Cells[2].Controls[0]).Text;
            string logTimeStr = ((TextBox)row.Cells[3].Controls[0]).Text;

            using (SqlConnection conn = new SqlConnection(connString))
            {
                string query = @"UPDATE MatchLogs 
                                 SET MatchId = @MatchId, LogMessage = @LogMessage, LogTime = @LogTime 
                                 WHERE LogId = @LogId";
                SqlCommand cmd = new SqlCommand(query, conn);

                cmd.Parameters.AddWithValue("@MatchId", matchIdStr);
                cmd.Parameters.AddWithValue("@LogMessage", logMessage);

                // Parse the edited LogTime, or fallback to current time if parsing fails
                DateTime parsedTime;
                if (DateTime.TryParse(logTimeStr, out parsedTime))
                {
                    cmd.Parameters.AddWithValue("@LogTime", parsedTime);
                }
                else
                {
                    cmd.Parameters.AddWithValue("@LogTime", DateTime.Now);
                }

                cmd.Parameters.AddWithValue("@LogId", logId);

                try
                {
                    conn.Open();
                    cmd.ExecuteNonQuery();

                    gvLogs.EditIndex = -1;
                    lblMessage.Text = "Log updated successfully!";
                    lblMessage.ForeColor = System.Drawing.Color.Green;
                }
                catch (SqlException ex)
                {
                    lblMessage.Text = "Error updating log (Check that Match ID exists): " + ex.Message;
                    lblMessage.ForeColor = System.Drawing.Color.Red;
                }
            }

            BindGrid();
        }

        // --- DELETE: Remove Log ---
        protected void gvLogs_RowDeleting(object sender, GridViewDeleteEventArgs e)
        {
            int logId = Convert.ToInt32(gvLogs.DataKeys[e.RowIndex].Value);

            using (SqlConnection conn = new SqlConnection(connString))
            {
                try
                {
                    conn.Open();

                    SqlCommand cmd = new SqlCommand("DELETE FROM MatchLogs WHERE LogId = @LogId", conn);
                    cmd.Parameters.AddWithValue("@LogId", logId);
                    cmd.ExecuteNonQuery();

                    lblMessage.Text = "Log entry deleted successfully!";
                    lblMessage.ForeColor = System.Drawing.Color.Green;
                }
                catch (SqlException ex)
                {
                    lblMessage.Text = "Error deleting log: " + ex.Message;
                    lblMessage.ForeColor = System.Drawing.Color.Red;
                }
            }
            BindGrid();
        }
    }
}