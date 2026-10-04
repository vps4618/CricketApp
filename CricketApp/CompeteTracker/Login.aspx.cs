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
    public partial class Login : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            // Clear session if they navigate back to login
            if (!IsPostBack)
            {
                Session.Clear();
            }
        }

        protected void btnLogin_Click(object sender, EventArgs e)
        {
            string configPassword = ConfigurationManager.AppSettings["TeamPassword"];

            if (txtPassword.Text != configPassword)
            {
                lblError.Text = "Invalid team password.";
                return;
            }

            string connString = ConfigurationManager.ConnectionStrings["CricketDb"].ConnectionString;

            using (SqlConnection con = new SqlConnection(connString))
            {
                // Look up the member by name
                string query = "SELECT MemberId FROM Members WHERE Name = @Name";
                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    cmd.Parameters.AddWithValue("@Name", txtUsername.Text.Trim());
                    con.Open();
                    object result = cmd.ExecuteScalar();

                    if (result != null)
                    {
                        // Login Success - Store details in Session
                        Session["MemberId"] = Convert.ToInt32(result);
                        Session["MemberName"] = txtUsername.Text.Trim();
                        Response.Redirect("MyTasks.aspx");
                    }
                    else
                    {
                        lblError.Text = "Username not found in database.";
                    }
                }
            }
        }
    }
}
