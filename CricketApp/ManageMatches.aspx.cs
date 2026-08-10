using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Web.UI.WebControls;

namespace CricketApp
{
    public partial class ManageMatches : System.Web.UI.Page
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
                LoadTeams();
                BindGrid();
            }
        }

        // --- Fetch teams for the Add Match DropDowns ---
        private void LoadTeams()
        {
            using (SqlConnection conn = new SqlConnection(connString))
            {
                SqlDataAdapter da = new SqlDataAdapter("SELECT TeamId, TeamName FROM Teams", conn);
                DataTable dt = new DataTable();
                da.Fill(dt);

                ddlTeam1.DataSource = dt;
                ddlTeam1.DataBind();

                ddlTeam2.DataSource = dt;
                ddlTeam2.DataBind();

                ddlTossWinner.DataSource = dt;
                ddlTossWinner.DataBind();
            }
        }

        // --- READ: Fetch data into GridView ---
        private void BindGrid()
        {
            using (SqlConnection conn = new SqlConnection(connString))
            {
                // Fetching all matches according to the schema
                string query = "SELECT MatchId, Team1Id, Team2Id, WinnerTeamId, IsLive, TossWinnerId FROM Matches ORDER BY MatchId DESC";
                SqlDataAdapter da = new SqlDataAdapter(query, conn);
                DataTable dt = new DataTable();
                da.Fill(dt);

                gvMatches.DataSource = dt;
                gvMatches.DataBind();
            }
        }

        // --- CREATE: Add a new Match ---
        protected void btnAddMatch_Click(object sender, EventArgs e)
        {
            using (SqlConnection conn = new SqlConnection(connString))
            {
                string query = @"INSERT INTO Matches (Team1Id, Team2Id, TossWinnerId, IsLive) 
                                 VALUES (@Team1Id, @Team2Id, @TossWinnerId, 0)";
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@Team1Id", ddlTeam1.SelectedValue);
                cmd.Parameters.AddWithValue("@Team2Id", ddlTeam2.SelectedValue);
                cmd.Parameters.AddWithValue("@TossWinnerId", ddlTossWinner.SelectedValue);

                conn.Open();
                cmd.ExecuteNonQuery();
            }

            lblMessage.Text = "Match created successfully!";
            lblMessage.ForeColor = System.Drawing.Color.Green;
            BindGrid();
        }

        // --- UPDATE: Trigger Edit Mode ---
        protected void gvMatches_RowEditing(object sender, GridViewEditEventArgs e)
        {
            gvMatches.EditIndex = e.NewEditIndex;
            BindGrid();
        }

        // --- UPDATE: Cancel Edit Mode ---
        protected void gvMatches_RowCancelingEdit(object sender, GridViewCancelEditEventArgs e)
        {
            gvMatches.EditIndex = -1;
            BindGrid();
        }

        // --- UPDATE: Save Changes ---
        protected void gvMatches_RowUpdating(object sender, GridViewUpdateEventArgs e)
        {
            int matchId = Convert.ToInt32(gvMatches.DataKeys[e.RowIndex].Value);
            GridViewRow row = gvMatches.Rows[e.RowIndex];

            // Accessing the BoundFields and CheckBoxField natively rendered by ASP.NET in Edit Mode
            string team1IdStr = ((TextBox)row.Cells[1].Controls[0]).Text;
            string team2IdStr = ((TextBox)row.Cells[2].Controls[0]).Text;
            string tossWinnerIdStr = ((TextBox)row.Cells[3].Controls[0]).Text;
            string winnerTeamIdStr = ((TextBox)row.Cells[4].Controls[0]).Text;
            bool isLive = ((CheckBox)row.Cells[5].Controls[0]).Checked;

            using (SqlConnection conn = new SqlConnection(connString))
            {
                string query = @"UPDATE Matches 
                                 SET Team1Id = @T1, Team2Id = @T2, TossWinnerId = @Toss, 
                                     WinnerTeamId = @Winner, IsLive = @IsLive 
                                 WHERE MatchId = @MatchId";
                SqlCommand cmd = new SqlCommand(query, conn);

                cmd.Parameters.AddWithValue("@T1", team1IdStr);
                cmd.Parameters.AddWithValue("@T2", team2IdStr);

                // Handle potentially nullable foreign keys
                cmd.Parameters.AddWithValue("@Toss", string.IsNullOrWhiteSpace(tossWinnerIdStr) ? (object)DBNull.Value : tossWinnerIdStr);
                cmd.Parameters.AddWithValue("@Winner", string.IsNullOrWhiteSpace(winnerTeamIdStr) ? (object)DBNull.Value : winnerTeamIdStr);

                cmd.Parameters.AddWithValue("@IsLive", isLive);
                cmd.Parameters.AddWithValue("@MatchId", matchId);

                conn.Open();
                cmd.ExecuteNonQuery();
            }

            gvMatches.EditIndex = -1;
            lblMessage.Text = "Match updated successfully!";
            lblMessage.ForeColor = System.Drawing.Color.Green;
            BindGrid();
        }

        // --- DELETE: Remove Match ---
        protected void gvMatches_RowDeleting(object sender, GridViewDeleteEventArgs e)
        {
            int matchId = Convert.ToInt32(gvMatches.DataKeys[e.RowIndex].Value);

            using (SqlConnection conn = new SqlConnection(connString))
            {
                try
                {
                    conn.Open();

                    // You must delete dependent foreign key rows in MatchLogs, MatchStateHistory, and MatchState first
                    SqlCommand delLogs = new SqlCommand("DELETE FROM MatchLogs WHERE MatchId = @MatchId", conn);
                    delLogs.Parameters.AddWithValue("@MatchId", matchId);
                    delLogs.ExecuteNonQuery();

                    SqlCommand delHistory = new SqlCommand("DELETE FROM MatchStateHistory WHERE MatchId = @MatchId", conn);
                    delHistory.Parameters.AddWithValue("@MatchId", matchId);
                    delHistory.ExecuteNonQuery();

                    SqlCommand delState = new SqlCommand("DELETE FROM MatchState WHERE MatchId = @MatchId", conn);
                    delState.Parameters.AddWithValue("@MatchId", matchId);
                    delState.ExecuteNonQuery();

                    // Finally, delete the Match
                    SqlCommand cmd = new SqlCommand("DELETE FROM Matches WHERE MatchId = @MatchId", conn);
                    cmd.Parameters.AddWithValue("@MatchId", matchId);
                    cmd.ExecuteNonQuery();

                    lblMessage.Text = "Match and associated data deleted successfully!";
                    lblMessage.ForeColor = System.Drawing.Color.Green;
                }
                catch (SqlException ex)
                {
                    lblMessage.Text = "Error deleting match: " + ex.Message;
                    lblMessage.ForeColor = System.Drawing.Color.Red;
                }
            }
            BindGrid();
        }
    }
}