using System.Collections.Generic;
using System.Data.Entity.Migrations;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;
using NoPassAssignment.Models;

namespace NoPassAssignment.Controllers
{
    [Authorize]
    public class AccountController : Controller
    {
        private ApplicationSignInManager _signInManager;
        private ApplicationUserManager _userManager;
        private ApplicationRoleManager _roleManager;
        private ApplicationDbContext _dbContext;

        public AccountController()
        {
        }

        public AccountController(ApplicationUserManager userManager, ApplicationSignInManager signInManager, ApplicationRoleManager roleManager, ApplicationDbContext dbContext)
        {
            UserManager = userManager;
            SignInManager = signInManager;
            RoleManager = roleManager;
            DbContext = dbContext;
        }
        public ApplicationDbContext DbContext
        {
            get
            {
                return _dbContext ?? HttpContext.GetOwinContext().Get<ApplicationDbContext>();
            }
            private set
            {
                _dbContext = value;
            }
        }
        public ApplicationRoleManager RoleManager
        {
            get
            {
                return _roleManager ?? HttpContext.GetOwinContext().Get<ApplicationRoleManager>();
            }
            private set
            {
                _roleManager = value;
            }
        }
        public ApplicationSignInManager SignInManager
        {
            get
            {
                return _signInManager ?? HttpContext.GetOwinContext().Get<ApplicationSignInManager>();
            }
            private set
            {
                _signInManager = value;
            }
        }
        public ApplicationUserManager UserManager
        {
            get
            {
                return _userManager ?? HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>();
            }
            private set
            {
                _userManager = value;
            }
        }
        
        // GET: /Account/Login
        [AllowAnonymous]
        public ActionResult Login(string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        // POST: /Account/Login
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Login(LoginViewModel model, string returnUrl)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var result = await SignInManager.PasswordSignInAsync(model.Username, model.Password, false, shouldLockout: false);
            switch (result)
            {
                case SignInStatus.Success:
                    var user = UserManager.FindByName(model.Username);
                    Session["UserName"] = user.UserName;
                    Session["UserId"] = user.Id;
                    CreateSessionRecord(user);
                    AddUserToActiveUsers(model.Username);


                    //HttpCookie myCookie = new HttpCookie("MyTestCookie");
                    //DateTime now = DateTime.Now;

                    //// Set the cookie value.
                    //myCookie.Value = now.ToString();
                    //// Set the cookie expiration date.
                    //myCookie.Expires = now.AddMinutes(1);

                    //// Add the cookie.
                    //Response.Cookies.Add(myCookie);

                    //SessionIDManager manager = new SessionIDManager();

                    //Session["UserID"] = user.Id;
                    //var role = UserManager.FindByName(user.UserName).Roles;
                    //Session["UserName"] = user.UserName;
                    //Session["UserRole"] = user.Roles;
                    //FormsAuthentication.SetAuthCookie(user.UserName, false);
                    var userRole = GetUserRole(user.UserName);
                    
                    if (userRole.Equals("NormalUser"))
                    {
                        return View("WelcomePageForNormalUser");
                    }
                    else if (userRole.Equals("MasterUser"))
                    {
                        ViewBag.ActiveUsers = GetActiveUsersWithoutCurrentUser();
                        return View("MasterPage");
                    }
                    Session.Abandon();
                    return RedirectToAction("Login", "Account");
                default:
                    return View("Error");
            }
        }

        private List<string> GetActiveUsersWithoutCurrentUser()
        {
            var activeUsers = (List<string>)HttpContext.Application["activeUsers"];
            var currentUser = Session["UserName"];
            activeUsers.RemoveAll(u => u == (string)currentUser);
            return activeUsers;
        }

        /*We know that each user has only one role assigned */
        private string GetUserRole(string username)
        {
            var listOfRoleIds = UserManager.FindByName(username).Roles.Select(x => x.RoleId).ToList();
            return RoleManager.FindById(listOfRoleIds[0]).Name;
        }

        private void CreateSessionRecord(ApplicationUser user)
        {
            SessionRecords session = new SessionRecords() { SessionId = Session.SessionID, UserId = user.Id, SessionExpired = 0 };
            DbContext.SessionRecords.AddOrUpdate(session);
            DbContext.SaveChanges();
        }

        private void AddUserToActiveUsers(string userName)
        {
            var activeUsers = (List<string>)HttpContext.Application["activeUsers"];
            activeUsers.Add(userName);
        }
        [HttpGet]
        public ActionResult CheckIfSessionExpired()
        {
            var currentUserId = (string)Session["UserId"];
            var result = DbContext.SessionRecords.SingleOrDefault(session => session.UserId == currentUserId);
           
           if(result != null && result.SessionExpired == 1)
            {
                RemoveSessionRecordFromDb(result);
                return View("Login");
            }
            return View(Request.RawUrl);

        }

        private void RemoveSessionRecordFromDb(SessionRecords sessionRecord)
        {
            DbContext.SessionRecords.Remove(sessionRecord);
            DbContext.SaveChanges();
        }

        [HttpPost]
        public ActionResult LogOut(string userName)
        {
            var userId = UserManager.FindByName(userName).Id;
            var result = DbContext.SessionRecords.SingleOrDefault(session => session.UserId == userId);
            if (result != null)
            {
                result.SessionExpired = 1;
                DbContext.SaveChanges();
            }
            //Session.Abandon();
            return View("SuccessfullyLoggedOut");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_userManager != null)
                {
                    _userManager.Dispose();
                    _userManager = null;
                }

                if (_signInManager != null)
                {
                    _signInManager.Dispose();
                    _signInManager = null;
                }
            }

            base.Dispose(disposing);
        }

        #region Helpers
        // Used for XSRF protection when adding external logins
        private const string XsrfKey = "XsrfId";


        internal class ChallengeResult : HttpUnauthorizedResult
        {
            public ChallengeResult(string provider, string redirectUri)
                : this(provider, redirectUri, null)
            {
            }

            public ChallengeResult(string provider, string redirectUri, string userId)
            {
                LoginProvider = provider;
                RedirectUri = redirectUri;
                UserId = userId;
            }

            public string LoginProvider { get; set; }
            public string RedirectUri { get; set; }
            public string UserId { get; set; }

            public override void ExecuteResult(ControllerContext context)
            {
                var properties = new AuthenticationProperties { RedirectUri = RedirectUri };
                if (UserId != null)
                {
                    properties.Dictionary[XsrfKey] = UserId;
                }
                context.HttpContext.GetOwinContext().Authentication.Challenge(properties, LoginProvider);
            }
        }
        #endregion
    }
}