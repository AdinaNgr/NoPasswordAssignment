using System.Data.Entity.Migrations;
using System.Linq;
using System.Web.Mvc;
using NoPassAssignment.Models;

namespace NoPassAssignment.Controllers
{
    public partial class AccountController
    {
        [HttpGet]
        public ActionResult CheckIfSessionExpired()
        {
            var currentUserId = (string)Session["UserId"];
            var result = DbContext.SessionRecords.SingleOrDefault(session => session.UserId == currentUserId);

            if (result != null && result.SessionExpired == 1)
            {
                RemoveSessionRecordFromDb(result);
                Session.Abandon();
                return View("Login");
            }
            return View(Request.RawUrl);
        }
        private void AddVariablesToCurrentSession(ApplicationUser user)
        {
            Session["UserName"] = user.UserName;
            Session["UserId"] = user.Id;
        }
        private void CreateSessionRecord(ApplicationUser user)
        {
            SessionRecords session = new SessionRecords() { SessionId = Session.SessionID, UserId = user.Id, SessionExpired = 0 };
            DbContext.SessionRecords.AddOrUpdate(session);
            DbContext.SaveChanges();
        }
        private void RemoveSessionRecordFromDb(SessionRecords sessionRecord)
        {
            DbContext.SessionRecords.Remove(sessionRecord);
            DbContext.SaveChanges();
        }
    }
}