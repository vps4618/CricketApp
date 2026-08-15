using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Web.UI.WebControls;

namespace CricketApp
{
    public partial class ManageMatchState : System.Web.UI.Page
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
                LoadDropdowns();
                BindGrid();
            }
        }

        // --- Fetch Matches and Teams for the DropDowns ---
        private void LoadDropdowns()
        {
            using (SqlConnection conn = new SqlConnection(connString))
            {
                conn.Open();

                // Load Matches
                SqlDataAdapter daMatches = new SqlDataAdapter("SELECT MatchId FROM Matches ORDER BY MatchId DESC", conn);
                DataTable dtMatches = new DataTable();
                daMatches.Fill(dtMatches);
                ddlMatchId.DataSource = dtMatches;
                ddlMatchId.DataBind();

                // Load Teams
                SqlDataAdapter daTeams = new SqlDataAdapter("SELECT TeamId, TeamName FROM Teams", conn);
                DataTable dtTeams = new DataTable();
                daTeams.Fill(dtTeams);
                ddlBattingTeamId.DataSource = dtTeams;
                ddlBattingTeamId.DataBind();
            }
        }

        // --- READ: Fetch data into GridView ---
        private void BindGrid()
        {
            using (SqlConnection conn = new SqlConnection(connString))
            {
                string query = "SELECT MatchId, BattingTeamId, TeamScore, Wickets, Overs, Balls, Innings, TargetScore, LastUpdated FROM MatchState ORDER BY MatchId DESC";
                SqlDataAdapter da = new SqlDataAdapter(query, conn);
                DataTable dt = new DataTable();
                da.Fill(dt);

                gvMatchState.DataSource = dt;
                gvMatchState.DataBind();
            }
        }

        // --- CREATE: Add a new Match State ---
        protected void btnAddMatchState_Click(object sender, EventArgs e)
        {
            if (ddlMatchId.SelectedValue == "")
            {
                lblMessage.Text = "You must select a Match ID.";
                lblMessage.ForeColor = System.Drawing.Color.Red;
                return;
            }

            using (SqlConnection conn = new SqlConnection(connString))
            {
                string query = @"INSERT INTO MatchState (MatchId, BattingTeamId, TeamScore, Wickets, Overs, Balls, LastUpdated, Innings, TargetScore) 
                                 VALUES (@MatchId, @BattingTeamId, @TeamScore, @Wickets, @Overs, @Balls, DATEADD(minute, 330, GETUTCDATE()), @Innings, @TargetScore)";
                SqlCommand cmd = new SqlCommand(query, conn);

                cmd.Parameters.AddWithValue("@MatchId", ddlMatchId.SelectedValue);
                cmd.Parameters.AddWithValue("@BattingTeamId", ddlBattingTeamId.SelectedValue);
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
                    lblMessage.Text = "Match State created successfully!";
                    lblMessage.ForeColor = System.Drawing.Color.Green;
                }
                catch (SqlException ex)
                {
                    // Specifically catch Primary Key violation (Cannot insert duplicate key in object 'dbo.MatchState')
                    if (ex.Number == 2627)
                    {
                        lblMessage.Text = "Error: A state record already exists for this Match ID. Please update the existing record below instead.";
                    }
                    else
                    {
                        lblMessage.Text = "Database Error: " + ex.Message;
                    }
                    lblMessage.ForeColor = System.Drawing.Color.Red;
                }
            }

            BindGrid();
        }

        // --- UPDATE: Trigger Edit Mode ---
        protected void gvMatchState_RowEditing(object sender, GridViewEditEventArgs e)
        {
            gvMatchState.EditIndex = e.NewEditIndex;
            BindGrid();
        }

        // --- UPDATE: Cancel Edit Mode ---
        protected void gvMatchState_RowCancelingEdit(object sender, GridViewCancelEditEventArgs e)
        {
            gvMatchState.EditIndex = -1;
            BindGrid();
        }

        // --- UPDATE: Save Changes ---
        protected void gvMatchState_RowUpdating(object sender, GridViewUpdateEventArgs e)
        {
            // MatchId is DataKey and remains ReadOnly
            int matchId = Convert.ToInt32(gvMatchState.DataKeys[e.RowIndex].Value);
            GridViewRow row = gvMatchState.Rows[e.RowIndex];

            // Access BoundFields
            string battingTeamIdStr = ((TextBox)row.Cells[1].Controls[0]).Text;
            string teamScoreStr = ((TextBox)row.Cells[2].Controls[0]).Text;
            string wicketsStr = ((TextBox)row.Cells[3].Controls[0]).Text;
            string oversStr = ((TextBox)row.Cells[4].Controls[0]).Text;
            string ballsStr = ((TextBox)row.Cells[5].Controls[0]).Text;
            string inningsStr = ((TextBox)row.Cells[6].Controls[0]).Text;
            string targetScoreStr = ((TextBox)row.Cells[7].Controls[0]).Text;

            using (SqlConnection conn = new SqlConnection(connString))
            {
                string query = @"UPDATE MatchState 
                                 SET BattingTeamId = @BattingTeamId, TeamScore = @TeamScore, 
                                     Wickets = @Wickets, Overs = @Overs, Balls = @Balls, 
                                     Innings = @Innings, TargetScore = @TargetScore, LastUpdated = DATEADD(minute, 330, GETUTCDATE())
                                 WHERE MatchId = @MatchId";
                SqlCommand cmd = new SqlCommand(query, conn);

                cmd.Parameters.AddWithValue("@BattingTeamId", string.IsNullOrWhiteSpace(battingTeamIdStr) ? (object)DBNull.Value : Convert.ToInt32(battingTeamIdStr));
                cmd.Parameters.AddWithValue("@TeamScore", Convert.ToInt32(teamScoreStr));
                cmd.Parameters.AddWithValue("@Wickets", Convert.ToInt32(wicketsStr));
                cmd.Parameters.AddWithValue("@Overs", Convert.ToInt32(oversStr));
                cmd.Parameters.AddWithValue("@Balls", Convert.ToInt32(ballsStr));
                cmd.Parameters.AddWithValue("@Innings", Convert.ToInt32(inningsStr));
                cmd.Parameters.AddWithValue("@TargetScore", Convert.ToInt32(targetScoreStr));
                cmd.Parameters.AddWithValue("@MatchId", matchId);

                try
                {
                    conn.Open();
                    cmd.ExecuteNonQuery();

                    gvMatchState.EditIndex = -1;
                    lblMessage.Text = "Match State updated successfully!";
                    lblMessage.ForeColor = System.Drawing.Color.Green;
                }
                catch (Exception ex)
                {
                    lblMessage.Text = "Error updating state: " + ex.Message;
                    lblMessage.ForeColor = System.Drawing.Color.Red;
                }
            }

            BindGrid();
        }

        // --- DELETE: Remove Match State ---
        protected void gvMatchState_RowDeleting(object sender, GridViewDeleteEventArgs e)
        {
            int matchId = Convert.ToInt32(gvMatchState.DataKeys[e.RowIndex].Value);

            using (SqlConnection conn = new SqlConnection(connString))
            {
                try
                {
                    conn.Open();

                    SqlCommand cmd = new SqlCommand("DELETE FROM MatchState WHERE MatchId = @MatchId", conn);
                    cmd.Parameters.AddWithValue("@MatchId", matchId);
                    cmd.ExecuteNonQuery();

                    lblMessage.Text = "Match State removed successfully!";
                    lblMessage.ForeColor = System.Drawing.Color.Green;
                }
                catch (SqlException ex)
                {
                    lblMessage.Text = "Error deleting state record: " + ex.Message;
                    lblMessage.ForeColor = System.Drawing.Color.Red;
                }
            }
            BindGrid();
        }
    }
}