using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Localactors.webapp.Areas.Admin.Controllers
{

    public class HomeController : ControllerBase
    {
        //
        // GET: /Admin/Home/

        public ActionResult Index()
        {
            return View();
        }

    }
}
