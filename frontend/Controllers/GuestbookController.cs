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
    public class GuestbookController : ControllerBase
    {


        //
        // GET: /Guestbook/

        public ViewResult Index()
        {
            var project_guestbook = db.project_guestbook.Include("project").Include("user");
            return View(project_guestbook.ToList());
        }

        //
        // GET: /Guestbook/Details/5

        public ViewResult Details(int id)
        {
            project_guestbook project_guestbook = db.project_guestbook.Single(p => p.GuestpostID == id);
            return View(project_guestbook);
        }

        //
        // GET: /Guestbook/Create

        public ActionResult Create()
        {
            ViewBag.ProjectID = new SelectList(db.projects, "ProjectID", "Title");
            ViewBag.UserID = new SelectList(db.users, "UserID", "Role");
            return View();
        } 

        //
        // POST: /Guestbook/Create

        [HttpPost]
        public ActionResult Create(project_guestbook project_guestbook)
        {
            if (ModelState.IsValid)
            {
                db.project_guestbook.AddObject(project_guestbook);
                db.SaveChanges();
                return RedirectToAction("Index");  
            }

            ViewBag.ProjectID = new SelectList(db.projects, "ProjectID", "Title", project_guestbook.ProjectID);
            ViewBag.UserID = new SelectList(db.users, "UserID", "Role", project_guestbook.UserID);
            return View(project_guestbook);
        }
        
        //
        // GET: /Guestbook/Edit/5
 
        public ActionResult Edit(int id)
        {
            project_guestbook project_guestbook = db.project_guestbook.Single(p => p.GuestpostID == id);
            ViewBag.ProjectID = new SelectList(db.projects, "ProjectID", "Title", project_guestbook.ProjectID);
            ViewBag.UserID = new SelectList(db.users, "UserID", "Role", project_guestbook.UserID);
            return View(project_guestbook);
        }

        //
        // POST: /Guestbook/Edit/5

        [HttpPost]
        public ActionResult Edit(project_guestbook project_guestbook)
        {
            if (ModelState.IsValid)
            {
                db.project_guestbook.Attach(project_guestbook);
                db.ObjectStateManager.ChangeObjectState(project_guestbook, EntityState.Modified);
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.ProjectID = new SelectList(db.projects, "ProjectID", "Title", project_guestbook.ProjectID);
            ViewBag.UserID = new SelectList(db.users, "UserID", "Role", project_guestbook.UserID);
            return View(project_guestbook);
        }

        //
        // GET: /Guestbook/Delete/5
 
        public ActionResult Delete(int id)
        {
            project_guestbook project_guestbook = db.project_guestbook.Single(p => p.GuestpostID == id);
            return View(project_guestbook);
        }

        //
        // POST: /Guestbook/Delete/5

        [HttpPost, ActionName("Delete")]
        public ActionResult DeleteConfirmed(int id)
        {            
            project_guestbook project_guestbook = db.project_guestbook.Single(p => p.GuestpostID == id);
            db.project_guestbook.DeleteObject(project_guestbook);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }
    }
}