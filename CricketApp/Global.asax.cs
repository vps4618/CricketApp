using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Optimization;
using System.Web.Routing;
using System.Web.Security;
using System.Web.SessionState;

namespace CricketApp
{
    public class Global : HttpApplication
    {
        void Application_Start(object sender, EventArgs e)
        {
            // Register your clean URL routes here
            RegisterRoutes(RouteTable.Routes);
        }

        static void RegisterRoutes(RouteCollection routes)
        {
            // MapPageRoute(RouteName, URL Pattern, Physical File)

            // Matches /default or /default/
            routes.MapPageRoute("DefaultRoute", "default", "~/Default.aspx");

            // Matches /admin or /admin/
            routes.MapPageRoute("AdminRoute", "admin", "~/Admin.aspx");

            // Matches /manageteams or /manageteams/
            routes.MapPageRoute("ManageTeamsRoute", "manageteams", "~/ManageTeams.aspx");
        }
    }
}