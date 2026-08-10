using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Web.UI.WebControls;

namespace CricketApp
{
    public partial class ManageMatchStateHistory : System.Web.UI.Page
    {
        string connString = ConfigurationManager.ConnectionStrings["CricketDb"].ConnectionString;

        protected void Page_Load(object sender, EventArgs e)
        {
            // 1. PASSWORD PROTECTION SECURITY CHECK
            if (Session["IsAdmin"] == null || (bool)Session["IsAdmin"] == false)
            {
                // Kick to Admin login screen if not authenticated
                Response.Redirect("admin");
            }

            if (!IsPostBack)
            {
                LoadMatches();
                BindGrid();
            }
        }

        // --- Fetch Matches for the DropDown ---
        private void LoadMatches()
        {
            using (SqlConnection conn = new SqlConnection(connString))
            {
                SqlDataAdapter daMatches = new SqlDataAdapter("SELECT MatchId FROM Matches ORDER BY MatchId DESC", conn);
                DataTable dtMatches = new DataTable();
                daMatches.Fill(dtMatches);

                ddlMatchId.DataSource = dtMatches;
                ddlMatchId.DataBind();
            }
        }

        // --- READ: Fetch data into GridView ---
        private void BindGrid()
        {
            using (SqlConnection conn = new SqlConnection(connString))
            {
                string query = "SELECT HistoryId, MatchId, TeamScore, Wickets, Overs, Balls, Innings, TargetScore FROM MatchStateHistory ORDER BY HistoryId DESC";
                SqlDataAdapter da = new SqlDataAdapter(query, conn);
                DataTable dt = new DataTable();
                da.Fill(dt);

                gvHistory.DataSource = dt;
                gvHistory.DataBind();
            }
        }

        // --- CREATE: Add a new History Record ---
        protected void btnAddHistory_Click(object sender, EventArgs e)
        {
            if (ddlMatchId.SelectedValue == "")
            {
                lblMessage.Text = "You must select a Match ID.";
                lblMessage.ForeColor = System.Drawing.Color.Red;
                return;
            }

            using (SqlConnection conn = new SqlConnection(connString))
            {
                string query = @"INSERT INTO MatchStateHistory (MatchId, TeamScore, Wickets, Overs, Balls, Innings, TargetScore) 
                                 VALUES (@MatchId, @TeamScore, @Wickets, @Overs, @Balls, @Innings, @TargetScore)";
                SqlCommand cmd = new SqlCommand(query, conn);

                cmd.Parameters.AddWithValue("@MatchId", ddlMatchId.SelectedValue);
                cmd.Parameters.AddWithValue("@TeamScore", string.IsNullOrWhiteSpace(txtTeamScore.Text) ? 0 : Convert.ToInt32(txtTeamScore.Text));
                cmd.Parameters.AddWithValue("@Wickets", string.IsNullOrWhiteSpace(txtWickets.Text) ? 0 : Convert.ToInt32(txtWickets.Text));
                cmd.Parameters.AddWithValue("@Overs", string.IsNullOrWhiteSpace(txtOvers.Text) ? 0 : Convert.ToInt32(txtOvers.Text));
                cmd.Parameters.AddWithValue("@Balls", string.IsNullOrWhiteSpace(txtBalls.Text) ? 0 : Convert.ToInt32(txtBalls.Text));
                cmd.Parameters.AddWithValue("@Innings", string.IsNullOrWhiteSpace(txtInnings.Text) ? 1 : Convert.ToInt32(txtInnings.Text));
                cmd.Parameters.AddWithValue("@TargetScore", string.IsNullOrWhiteSpace(txtTargetScore.Text) ? 0 : Convert.ToInt32(txtTargetScore.Text));

                try
                {
                    conn.Open();
                    cmd.ExecuteNonQuery();
                    lblMessage.Text = "History record added successfully!";
                    lblMessage.ForeColor = System.Drawing.Color.Green;
                }
                catch (SqlException ex)
                {
                    lblMessage.Text = "Database Error: " + ex.Message;
                    lblMessage.ForeColor = System.Drawing.Color.Red;
                }
            }

            BindGrid();
        }

        // --- UPDATE: Trigger Edit Mode ---
        protected void gvHistory_RowEditing(object sender, GridViewEditEventArgs e)
        {
            gvHistory.EditIndex = e.NewEditIndex;
            BindGrid();
        }

        // --- UPDATE: Cancel Edit Mode ---
        protected void gvHistory_RowCancelingEdit(object sender, GridViewCancelEditEventArgs e)
        {
            gvHistory.EditIndex = -1;
            BindGrid();
        }

        // --- UPDATE: Save Changes ---
        protected void gvHistory_RowUpdating(object sender, GridViewUpdateEventArgs e)
        {
            // HistoryId is DataKey and remains ReadOnly
            int historyId = Convert.ToInt32(gvHistory.DataKeys[e.RowIndex].Value);
            GridViewRow row = gvHistory.Rows[e.RowIndex];

            // Access BoundFields
            string matchIdStr = ((TextBox)row.Cells[1].Controls[0]).Text;
            string teamScoreStr = ((TextBox)row.Cells[2].Controls[0]).Text;
            string wicketsStr = ((TextBox)row.Cells[3].Controls[0]).Text;
            string oversStr = ((TextBox)row.Cells[4].Controls[0]).Text;
            string ballsStr = ((TextBox)row.Cells[5].Controls[0]).Text;
            string inningsStr = ((TextBox)row.Cells[6].Controls[0]).Text;
            string targetScoreStr = ((TextBox)row.Cells[7].Controls[0]).Text;

            using (SqlConnection conn = new SqlConnection(connString))
            {
                string query = @"UPDATE MatchStateHistory 
                                 SET MatchId = @MatchId, TeamScore = @TeamScore, 
                                     Wickets = @Wickets, Overs = @Overs, Balls = @Balls, 
                                     Innings = @Innings, TargetScore = @TargetScore
                                 WHERE HistoryId = @HistoryId";
                SqlCommand cmd = new SqlCommand(query, conn);

                cmd.Parameters.AddWithValue("@MatchId", Convert.ToInt32(matchIdStr));
                cmd.Parameters.AddWithValue("@TeamScore", Convert.ToInt32(teamScoreStr));
                cmd.Parameters.AddWithValue("@Wickets", Convert.ToInt32(wicketsStr));
                cmd.Parameters.AddWithValue("@Overs", Convert.ToInt32(oversStr));
                cmd.Parameters.AddWithValue("@Balls", Convert.ToInt32(ballsStr));
                cmd.Parameters.AddWithValue("@Innings", Convert.ToInt32(inningsStr));
                cmd.Parameters.AddWithValue("@TargetScore", Convert.ToInt32(targetScoreStr));
                cmd.Parameters.AddWithValue("@HistoryId", historyId);

                try
                {
                    conn.Open();
                    cmd.ExecuteNonQuery();

                    gvHistory.EditIndex = -1;
                    lblMessage.Text = "History record updated successfully!";
                    lblMessage.ForeColor = System.Drawing.Color.Green;
                }
                catch (Exception ex)
                {
                    lblMessage.Text = "Error updating history record: " + ex.Message;
                    lblMessage.ForeColor = System.Drawing.Color.Red;
                }
            }

            BindGrid();
        }

        // --- DELETE: Remove History Record ---
        protected void gvHistory_RowDeleting(object sender, GridViewDeleteEventArgs e)
        {
            int historyId = Convert.ToInt32(gvHistory.DataKeys[e.RowIndex].Value);

            using (SqlConnection conn = new SqlConnection(connString))
            {
                try
                {
                    conn.Open();

                    SqlCommand cmd = new SqlCommand("DELETE FROM MatchStateHistory WHERE HistoryId = @HistoryId", conn);
                    cmd.Parameters.AddWithValue("@HistoryId", historyId);
                    cmd.ExecuteNonQuery();

                    lblMessage.Text = "History record deleted successfully!";
                    lblMessage.ForeColor = System.Drawing.Color.Green;
                }
                catch (SqlException ex)
                {
                    lblMessage.Text = "Error deleting history record: " + ex.Message;
                    lblMessage.ForeColor = System.Drawing.Color.Red;
                }
            }
            BindGrid();
        }
    }
}