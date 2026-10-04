using System;
using System.Data;
using System.Data.SqlClient;
using System.Configuration;
using System.Web.UI.WebControls;


namespace CricketApp.CompeteTracker
{
    public partial class Default : System.Web.UI.Page
    {
        string connString = ConfigurationManager.ConnectionStrings["CricketDb"].ConnectionString;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                LoadDashboard();
            }
        }

        protected void DashboardTimer_Tick(object sender, EventArgs e)
        {
            // This fires every 5 seconds via AJAX
            LoadDashboard();
        }

        private void LoadDashboard()
        {
            using (SqlConnection con = new SqlConnection(connString))
            {
                SqlDataAdapter da = new SqlDataAdapter("SELECT MemberId, Name FROM Members", con);
                DataTable dtMembers = new DataTable();
                da.Fill(dtMembers);

                rptMembers.DataSource = dtMembers;
                rptMembers.DataBind();
            }
        }

        protected void rptMembers_ItemDataBound(object sender, RepeaterItemEventArgs e)
        {
            if (e.Item.ItemType == ListItemType.Item || e.Item.ItemType == ListItemType.AlternatingItem)
            {
                // Get the current member ID
                DataRowView drv = (DataRowView)e.Item.DataItem;
                int memberId = Convert.ToInt32(drv["MemberId"]);

                // Find the nested repeater
                Repeater rptQuestions = (Repeater)e.Item.FindControl("rptQuestions");

                // Load questions for this member
                using (SqlConnection con = new SqlConnection(connString))
                {
                    SqlCommand cmd = new SqlCommand("SELECT QuestionTitle, Status FROM Questions WHERE AssignedMemberId = @MemberId ORDER BY ISNULL(ModifiedDate, CreatedDate) DESC", con);
                    cmd.Parameters.AddWithValue("@MemberId", memberId);
                    SqlDataAdapter da = new SqlDataAdapter(cmd);
                    DataTable dtQuestions = new DataTable();
                    da.Fill(dtQuestions);

                    rptQuestions.DataSource = dtQuestions;
                    rptQuestions.DataBind();
                }
            }
        }

        // Helper method for Bootstrap badge colors
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