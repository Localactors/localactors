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
    public class ProjectsController : ControllerBase
    {

        public ViewResult Index()
        {
            var projects = db.projects.Include("country").Include("user").Include("project_photo");
            return View(projects.ToList());
        }

        //
        // GET: /Projects/Details/5

        public ViewResult Details(int id)
        {
            project project = db.projects.Include("country").Include("user").Include("project_photo").Single(p => p.ProjectID == id);
            return View(project);
        }

        ////
        //// GET: /Projects/Create

        //public ActionResult Create()
        //{
        //    ViewBag.CountryID = new SelectList(db.countries, "CountryID", "Code");
        //    ViewBag.UserID = new SelectList(db.users, "UserID", "Role");
        //    return View();
        //} 

        ////
        //// POST: /Projects/Create

        //[HttpPost]
        //public ActionResult Create(project project)
        //{
        //    if (ModelState.IsValid)
        //    {
        //        db.projects.AddObject(project);
        //        db.SaveChanges();
        //        return RedirectToAction("Index");  
        //    }

        //    ViewBag.CountryID = new SelectList(db.countries, "CountryID", "Code", project.CountryID);
        //    ViewBag.UserID = new SelectList(db.users, "UserID", "Role", project.UserID);
        //    return View(project);
        //}
        
        ////
        //// GET: /Projects/Edit/5
 
        //public ActionResult Edit(int id)
        //{
        //    project project = db.projects.Single(p => p.ProjectID == id);
        //    ViewBag.CountryID = new SelectList(db.countries, "CountryID", "Code", project.CountryID);
        //    ViewBag.UserID = new SelectList(db.users, "UserID", "Role", project.UserID);
        //    return View(project);
        //}

        ////
        //// POST: /Projects/Edit/5

        //[HttpPost]
        //public ActionResult Edit(project project)
        //{
        //    if (ModelState.IsValid)
        //    {
        //        db.projects.Attach(project);
        //        db.ObjectStateManager.ChangeObjectState(project, EntityState.Modified);
        //        db.SaveChanges();
        //        return RedirectToAction("Index");
        //    }
        //    ViewBag.CountryID = new SelectList(db.countries, "CountryID", "Code", project.CountryID);
        //    ViewBag.UserID = new SelectList(db.users, "UserID", "Role", project.UserID);
        //    return View(project);
        //}

        ////
        //// GET: /Projects/Delete/5
 
        //public ActionResult Delete(int id)
        //{
        //    project project = db.projects.Single(p => p.ProjectID == id);
        //    return View(project);
        //}

        ////
        //// POST: /Projects/Delete/5

        //[HttpPost, ActionName("Delete")]
        //public ActionResult DeleteConfirmed(int id)
        //{            
        //    project project = db.projects.Single(p => p.ProjectID == id);
        //    db.projects.DeleteObject(project);
        //    db.SaveChanges();
        //    return RedirectToAction("Index");
        //}

        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }
    }
}