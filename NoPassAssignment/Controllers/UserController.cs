using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Entity;
using System.Linq;
using Microsoft.AspNet.Identity;
using NoPassAssignment.Models;

namespace NoPassAssignment.Controllers
{
    public partial class AccountController
    {
        private void ModifyUserLoginFailuresAttempts(ApplicationUser user)
        {
            IncreaseAccessFailedCount(user);
            ViewBag.Message = "Invalid login attempt";
            if (EnableLockoutIfNecessary(user))
            {
                ViewBag.Message = "Too many login failures! User locked";
            }
        }
        private void IncreaseAccessFailedCount(ApplicationUser user)
        {
            user.AccessFailedCount += 1;
            DbContext.Entry(user).State = EntityState.Modified;
            DbContext.SaveChanges();
        }
        private Boolean EnableLockoutIfNecessary(ApplicationUser user)
        {
            Boolean shouldBeLocked = false;
            var accessFailedCount = user.AccessFailedCount;
            if (accessFailedCount > Int32.Parse(ConfigurationSettings.AppSettings.Get("MaximumLoginAttempts")))
            {
                shouldBeLocked = true;
                user.LockoutEnabled = true;
                var lockoutEndDate = DateTime.Now.AddHours(Int32.Parse(ConfigurationSettings.AppSettings.Get("NoHoursLockUser")));
                user.LockoutEndDateUtc = lockoutEndDate;
                DbContext.Entry(user).State = EntityState.Modified;
                DbContext.SaveChanges();
            }
            return shouldBeLocked;
        }
        private bool UserCorrectlyIntroducedSecurityCode(LoginViewModel model)
        {
            return model.SecurityCode.Equals(Session["securityCode"].ToString());
        }
        private List<string> GetActiveUsersWithoutUser(string userName)
        {
            var activeUsers = (List<string>)HttpContext.Application["activeUsers"];
            activeUsers.RemoveAll(u => u == userName);
            return activeUsers;
        }
        private bool ShouldBeUnlocked(ApplicationUser user)
        {
            if (user.LockoutEnabled)
            {
                DateTime endDate = (DateTime)user.LockoutEndDateUtc;
                var currentDate = DateTime.Now;
                var difference = endDate - currentDate;

                if (difference.Hours >= Int32.Parse(ConfigurationSettings.AppSettings.Get("NoHoursLockUser")))
                {
                    user.LockoutEnabled = false;
                    user.AccessFailedCount = 0;
                    DbContext.Entry(user).State = EntityState.Modified;
                    DbContext.SaveChanges();
                    return true;
                }
            }
            return false;
        }
        /* We know that each user has only one role assigned */
        private string GetUserRole(string username)
        {
            var listOfRoleIds = UserManager.FindByName(username).Roles.Select(x => x.RoleId).ToList();
            return RoleManager.FindById(listOfRoleIds[0]).Name;
        }
        private void AddUserToActiveUsers(string userName)
        {
            var activeUsers = (List<string>)HttpContext.Application["activeUsers"];
            activeUsers.Add(userName);
        }
    }
}