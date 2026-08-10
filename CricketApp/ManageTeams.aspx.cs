using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Web.UI.WebControls;

namespace CricketApp
{
    public partial class ManageTeams : System.Web.UI.Page
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
                BindGrid();
            }
        }

        // --- READ: Fetch data into GridView ---
        private void BindGrid()
        {
            using (SqlConnection conn = new SqlConnection(connString))
            {
                SqlDataAdapter da = new SqlDataAdapter("SELECT TeamId, TeamName FROM Teams", conn);
                DataTable dt = new DataTable();
                da.Fill(dt);
                gvTeams.DataSource = dt;
                gvTeams.DataBind();
            }
        }

        // --- CREATE: Add a new Team ---
        protected void btnAddTeam_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtNewTeam.Text))
            {
                lblMessage.Text = "Team name cannot be empty.";
                lblMessage.ForeColor = System.Drawing.Color.Red;
                return;
            }

            using (SqlConnection conn = new SqlConnection(connString))
            {
                string query = "INSERT INTO Teams (TeamName) VALUES (@TeamName)";
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@TeamName", txtNewTeam.Text.Trim());

                conn.Open();
                cmd.ExecuteNonQuery();
            }

            txtNewTeam.Text = "";
            lblMessage.Text = "Team added successfully!";
            lblMessage.ForeColor = System.Drawing.Color.Green;
            BindGrid();
        }

        // --- UPDATE: Trigger Edit Mode ---
        protected void gvTeams_RowEditing(object sender, GridViewEditEventArgs e)
        {
            gvTeams.EditIndex = e.NewEditIndex;
            BindGrid();
        }

        // --- UPDATE: Cancel Edit Mode ---
        protected void gvTeams_RowCancelingEdit(object sender, GridViewCancelEditEventArgs e)
        {
            gvTeams.EditIndex = -1;
            BindGrid();
        }

        // --- UPDATE: Save Changes ---
        protected void gvTeams_RowUpdating(object sender, GridViewUpdateEventArgs e)
        {
            int teamId = Convert.ToInt32(gvTeams.DataKeys[e.RowIndex].Value);
            TextBox txtEditTeamName = (TextBox)gvTeams.Rows[e.RowIndex].FindControl("txtEditTeamName");

            using (SqlConnection conn = new SqlConnection(connString))
            {
                string query = "UPDATE Teams SET TeamName = @TeamName WHERE TeamId = @TeamId";
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@TeamName", txtEditTeamName.Text.Trim());
                cmd.Parameters.AddWithValue("@TeamId", teamId);

                conn.Open();
                cmd.ExecuteNonQuery();
            }

            gvTeams.EditIndex = -1; // Exit edit mode
            lblMessage.Text = "Team updated successfully!";
            lblMessage.ForeColor = System.Drawing.Color.Green;
            BindGrid();
        }

        // --- DELETE: Remove Team ---
        protected void gvTeams_RowDeleting(object sender, GridViewDeleteEventArgs e)
        {
            int teamId = Convert.ToInt32(gvTeams.DataKeys[e.RowIndex].Value);

            using (SqlConnection conn = new SqlConnection(connString))
            {
                try
                {
                    string query = "DELETE FROM Teams WHERE TeamId = @TeamId";
                    SqlCommand cmd = new SqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@TeamId", teamId);

                    conn.Open();
                    cmd.ExecuteNonQuery();

                    lblMessage.Text = "Team deleted successfully!";
                    lblMessage.ForeColor = System.Drawing.Color.Green;
                }
                catch (SqlException)
                {
                    // If a team is already associated with a match in the 'Matches' table, 
                    // SQL will block the delete due to a Foreign Key constraint.
                    lblMessage.Text = "Cannot delete this team because it has already played in a match.";
                    lblMessage.ForeColor = System.Drawing.Color.Red;
                }
            }
            BindGrid();
        }
    }
}