using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Drawing.Text;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using NoPassAssignment.Models;

namespace NoPassAssignment.Controllers
{
    [Authorize]
    public partial class AccountController : Controller
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

        [AllowAnonymous]
        public ActionResult Login(string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Login(LoginViewModel model, string returnUrl)
        {
            if (HttpContext.Application[Request.UserHostAddress] != null)
            {
                var incrementalDelay = HttpContext.Application[Request.UserHostAddress];
                await Task.Delay((int) incrementalDelay * 1000);
            }
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            if (UserCorrectlyIntroducedSecurityCode(model))
            {
                var result =
                    await SignInManager.PasswordSignInAsync(model.Username, model.Password, false, false);
                switch (result)
                {
                    case SignInStatus.Success:
                        var user = UserManager.FindByName(model.Username);
                        return SignIn(user);

                    case SignInStatus.LockedOut:
                        IncrementDelay();
                        user = UserManager.FindByName(model.Username);
                        if (ShouldBeUnlocked(user))
                        {
                            return SignIn(user);
                        }
                        ViewBag.Message = "User locked";
                        return View(model);

                    default:
                        IncrementDelay();
                        user = UserManager.FindByName(model.Username);
                        ViewBag.Message = "Failed login. Unauthorized user";
                        if (user != null)
                        {
                            ModifyUserLoginFailuresAttempts(user);
                        }
                        return View(model);
                }
            }
            var currentUser = UserManager.FindByName(model.Username);
            ViewBag.Message = "Invalid security code.";
            if (currentUser != null)
            {
                ModifyUserLoginFailuresAttempts(currentUser);
            }
            IncrementDelay();
            return View(model);
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
            return View("SuccessfullyLoggedOut");
        }
        private ViewResult SignIn(ApplicationUser user)
        {
            // reset incremental delay on successful login
            if (HttpContext.Application[Request.UserHostAddress] != null)
            {
                HttpContext.Application.Remove(Request.UserHostAddress);
            }
            AddVariablesToCurrentSession(user);
            CreateSessionRecord(user);
            AddUserToActiveUsers(user.UserName);

            var userRole = GetUserRole(user.UserName);

            if (userRole.Equals("NormalUser"))
            {
                return View("WelcomePageForNormalUser");
            }
            if (userRole.Equals("MasterUser"))
            {
                ViewBag.ActiveUsers = GetActiveUsersWithoutUser((string)Session["UserName"]);
                return View("MasterPage");
            }
            ViewBag.Message = "Failed login. Unauthorized user";
            return View("Login");
        }

        private void IncrementDelay()
        {
            var incrementalDelay = HttpContext.Application[Request.UserHostAddress] == null
                ? 1
                : (int)HttpContext.Application[Request.UserHostAddress] * 2;
            HttpContext.Application[Request.UserHostAddress] = incrementalDelay;
        }
        [AllowAnonymous]
        public ActionResult Captcha()
        {
            var bitmap = new Bitmap(80, 40);
            var objGraphics = Graphics.FromImage(bitmap);
            objGraphics.Clear(Color.DimGray);
            objGraphics.TextRenderingHint = TextRenderingHint.AntiAlias;

            var font = new Font("Calibri", 14, FontStyle.Bold);
            var securityCode = "";
            var myIntArray = new int[5];
            int x;

            var autoRand = new Random();
            for (x = 0; x < 5; x++)
            {
                myIntArray[x] = Convert.ToInt32(autoRand.Next(0, 9));
                securityCode += (myIntArray[x].ToString());
            }
            Session.Add("securityCode", securityCode);
            objGraphics.DrawString(securityCode, font, Brushes.White, 4, 4);
            Response.ContentType = "image/GIF";
            bitmap.Save(Response.OutputStream, ImageFormat.Gif);

            font.Dispose();
            objGraphics.Dispose();
            bitmap.Dispose();

            return new EmptyResult();
        }
    }
}