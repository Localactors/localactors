﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Web.Mvc;
using System.Linq;
using Localactors.entities;

namespace Localactors.webapp.Controllers
{
    public class HomeController : ControllerBase
    {
        public class HomeModel {
            public List<user> publishers { get; set; }
            public List<project> projects { get; set; } 
        }

        public ActionResult Index() {

            HomeModel model = new HomeModel();
            model.publishers = db.users.Where(x => x.Role == "publisher" && x.Enabled).ToList();
            model.projects = db.projects.OrderByDescending("ProjectID" ).Where(x=>x.Enabled).Take(5).ToList();


            return View(model);
        }

        

        public ActionResult HealthCheck() {
            return Content("OK");
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

        public ActionResult CallForProjects()
        {
            return View();
        }
        public ActionResult Guidelines() {
            return View();
        }
        public ActionResult OurApproach()
        {
            return View();
        }
        public ActionResult HowTo()
        {
            return View();
        }


        public ActionResult fourofour() {
            return View();
        }

    }
}
