using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Localactors.webapp.Areas.Publisher.Controllers
{
    [Authorize]
    public class HomeController : ControllerBase
    {
        public ActionResult Index() {
            var user = db.users.FirstOrDefault(x => x.UserName == User.Identity.Name);
            if(user == null) {
                return RedirectToAction("Index","Home",new{area=""});
            }
            var projects = db.projects.Where(x=>x.UserID == user.UserID).Include("country").Include("user");
            return View(projects.ToList());
        }
    }
}
