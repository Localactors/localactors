using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Localactors.webapp.Controllers
{
    public class ErrorsController : Controller
    {
        //
        // GET: /Errors/

        public ActionResult FourOFour()
        {
            return View();
        }

        public ActionResult Fivehundred()
        {
            return View();
        }

    }
}
