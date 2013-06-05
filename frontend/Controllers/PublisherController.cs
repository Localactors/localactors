using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Localactors.entities;

namespace Localactors.webapp.Controllers
{ 
    public class PublisherController : ControllerBase
    {


        public ViewResult Index()
        {
            var users = db.users.Include("country");
            return View(users.ToList());
        }

        public ViewResult Details(int id)
        {
            user user = db.users.Single(u => u.UserID == id);
            return View(user);
        }


        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }
    }
}