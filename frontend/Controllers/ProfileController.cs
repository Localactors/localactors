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
    
    public class ProfileModel {
        public user user { get; set; }
        public List<project> projects { get; set; }
        public List<donation> donations { get; set; }
        public List<update> updates { get; set; }
    }

    public class ProfileController : ControllerBase
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

        [Authorize]
        public ViewResult Feed()
        {
            user user = db.users.Single(u => u.UserName == User.Identity.Name);

            ProfileModel model = new ProfileModel();
            model.user = db.users.Single(u => u.UserName == User.Identity.Name);
            model.projects = model.user.donations.Select(x => x.project).Distinct().ToList();
            model.donations = model.user.donations.ToList();
            model.updates = model.user.donations.Select(x => x.project).SelectMany(x => x.updates).OrderByDescending(x=>x.UpdateID).Take(5).ToList();

            return View(model);
        }

        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }
    }
}