using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using System.Web.Security;
using System.Web.SessionState;
using Microsoft.Owin.Security.Infrastructure;

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

        void Session_Start(object sender, EventArgs e)
        {
            //Session["GUIDSessionID"] = Guid.NewGuid().ToString();
            //var activeSessions = (System.Collections.Generic.List<string>)Application["activeSessions"];
            //activeSessions.Add(this.Session.SessionID);
            //var nr = Membership.GetNumberOfUsersOnline();
        }

        void Session_End(object sender, EventArgs e)
        {
            var activeUsers = (System.Collections.Generic.List<string>) Application["activeUsers"];
            activeUsers.Remove(this.Session.SessionID);
        }
        //protected void Application_AuthenticateRequest(Object sender, EventArgs e)
        //{
        //    //if (User.Identity.IsAuthenticated)
        //    //{
        //    //    //add your ckeck here

        //    //    //if (Usernames.Contains(User.Identity.Name))
        //    //    //{
        //    //    //    Session.Abandon();
        //    //    //    FormsAuthentication.SignOut();
        //    //    //}
        //    //}
        //}
        //protected void Application_PreRequestHandlerExecute(object sender, EventArgs e)
        //{
        //    ForceLogoutIfSessionExpired();
        //}

        //private void ForceLogoutIfSessionExpired()
        //{
        //    if (Context.Handler is IRequiresSessionState)
        //    {
        //        if (Request.IsAuthenticated)
        //        {
        //            if (HttpContext.Current.Session["name"] == null)
        //            {
        //                //AuthenticationHandler.SignOut(Response);
        //                Response.Redirect(FormsAuthentication.LoginUrl, true);
        //            }
        //        }
        //    }
        //}
    }
}
