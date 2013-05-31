using System;
using System.Web.Mvc;
using System.Linq;

namespace Localactors.webapp.Controllers
{
    public class HomeController : ControllerBase
    {
        public ActionResult Index() {
            return View();
        }

        public ActionResult Login()
        {
            return RedirectToAction("Login","Account");
        }

        public ActionResult Privacy() {
            return View();
        }

        public ActionResult TOS()
        {
            return View();
        }


        public ActionResult fourofour() {
            return View();
        }

    }
}
