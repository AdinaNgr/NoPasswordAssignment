using System;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;

namespace NoPassAssignment
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
        }

        void Application_Start(object sender, EventArgs e)
        {
            Application["activeUsers"] = new System.Collections.Generic.List<string>();
            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
        }

        void Session_End(object sender, EventArgs e)
        {
            var activeUsers = (System.Collections.Generic.List<string>) Application["activeUsers"];
            activeUsers.RemoveAll(u => u == (string) Session["UserName"]);
        }
        
    }
}
