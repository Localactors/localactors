using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Localactors.entities;

namespace Localactors.webapp.Areas.Admin.Controllers
{ 
    public class UserRolesController : ControllerBase
    {


        //
        // GET: /Admin/UserRoles/

        public ViewResult Index()
        {
            return View(db.user_roles.ToList());
        }


        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }
    }
}