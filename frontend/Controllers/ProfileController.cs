using System;
using System.Collections.Generic;
using System.Configuration;
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
        [ChildActionOnly]
        [OutputCache(Duration = 60, VaryByParam = "username")]
        public PartialViewResult ProfileBar(string username) {
            var user = db.users.FirstOrDefault(x => x.UserName == username);

            return PartialView("_ProfileBar", user);
        }

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
        public ViewResult Feed(int page=1) {
            var realpage = page - 1;
            int pagesize = int.Parse(ConfigurationManager.AppSettings["Pagesize_UserUpdates"]);
            int skip = realpage * pagesize;
            int take = pagesize;

            user user = db.users.Single(u => u.UserName == User.Identity.Name);

            ProfileModel model = new ProfileModel();
            model.user = db.users.Single(u => u.UserName == User.Identity.Name);
            //model.projects = model.user.donations.Select(x => x.project).Distinct().ToList();
            //model.donations = model.user.donations.ToList();
            model.updates = model.user.followedProjects.SelectMany(x => x.updates).OrderByDescending(x => x.UpdateID).Skip(skip).Take(take).ToList();

            ViewBag.page = page;
            return View(model);
        }

        [Authorize]
        public ViewResult Settings() {
            return View(CurrentUser);
        }
        [Authorize]
        [HttpPost]
        public ViewResult Settings(user model)
        {
            return View(CurrentUser);
        }


        

        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }
    }
}