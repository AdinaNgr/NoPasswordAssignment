using NoPassAssignment.Models;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.Owin;
using Owin;
[assembly: OwinStartupAttribute(typeof(NoPassAssignment.Startup))]
namespace NoPassAssignment
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
            CreateRolesAndUsers();
        }

        private void CreateRolesAndUsers()
        {
            var context = new ApplicationDbContext();
            var roleManager = new RoleManager<IdentityRole>(new RoleStore<IdentityRole>(context));
            var userManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(context));

            if (!roleManager.RoleExists("MasterUser"))
            {
                var role =
                    new IdentityRole
                    {
                        Name = "MasterUser"
                    };
                roleManager.Create(role);

                var user = new ApplicationUser { UserName = "MasterUser" };
                var userPwd = "masterUser";
                var createdUser = userManager.Create(user, userPwd);

                if (createdUser.Succeeded)
                {
                    userManager.AddToRole(user.Id, "MasterUser");
                }
            }

            if (!roleManager.RoleExists("NormalUser"))
            {
                var role =
                    new IdentityRole
                    {
                        Name = "NormalUser"
                    };
                roleManager.Create(role);
                var user = new ApplicationUser { UserName = "user1" };
                var userPwd = "user1p";
                var createdUser = userManager.Create(user, userPwd);
                if (createdUser.Succeeded)
                {
                    userManager.AddToRole(user.Id, "NormalUser");
                }

                user = new ApplicationUser { UserName = "user2" };
                userPwd = "user2p";
                var chkUser2 = userManager.Create(user, userPwd);

                if (chkUser2.Succeeded)
                {
                    userManager.AddToRole(user.Id, "NormalUser");
                }
            }
        }
    }
}
