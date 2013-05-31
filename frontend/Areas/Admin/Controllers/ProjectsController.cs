using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Mvc;
using Localactors.entities;

namespace Localactors.webapp.Areas.Admin.Controllers
{
    [Authorize(Roles = "admin")]
    public class ProjectsController : ControllerBase
    {
      
        //
        // GET: /Admin/Projects/

        public ViewResult Index()
        {
            var projects = db.projects.Include("country").Include("user");
            return View(projects.ToList());
        }

        //
        // GET: /Admin/Projects/Details/5

        public ViewResult Details(int id)
        {
            project project = db.projects.Single(p => p.ProjectID == id);
            return View(project);
        }

        //
        // GET: /Admin/Projects/Create

        public ActionResult Create()
        {
            ViewBag.CountryID = new SelectList(db.countries, "CountryID", "Code");
            ViewBag.UserID = new SelectList(db.users.Where(x => x.Role == "admin" || x.Role == "publisher"), "UserID", "UserName");
            return View();
        } 

        //
        // POST: /Admin/Projects/Create

        [HttpPost]
        public ActionResult Create(project project)
        {
            if (ModelState.IsValid)
            {
                db.projects.AddObject(project);
                db.SaveChanges();
                //return RedirectToAction("Index");  
                return RedirectToAction("Create","Update",new{projectid = project.ProjectID});  
            }

            ViewBag.CountryID = new SelectList(db.countries, "CountryID", "Code", project.CountryID);
            ViewBag.UserID = new SelectList(db.users.Where(x => x.Role == "admin" || x.Role == "publisher"), "UserID", "UserName", project.UserID);
            return View(project);
        }
        
        //
        // GET: /Admin/Projects/Edit/5
 
        public ActionResult Edit(int id)
        {
            project project = db.projects.Single(p => p.ProjectID == id);
            ViewBag.Supporters = new SelectList(db.users.Where(x => x.Role == "admin" || x.Role == "supporter"), "UserID", "UserName");
            ViewBag.CountryID = new SelectList(db.countries, "CountryID", "Code", project.CountryID);
            ViewBag.UserID = new SelectList(db.users.Where(x => x.Role == "admin" || x.Role == "publisher"), "UserID", "UserName", project.UserID);
            ViewBag.Tags = db.tags;
            return View(project);
        }

        //
        // POST: /Admin/Projects/Edit/5

        [HttpPost]
        public ActionResult Edit(project project)
        {
            if (ModelState.IsValid)
            {
                db.projects.Attach(project);
                db.ObjectStateManager.ChangeObjectState(project, EntityState.Modified);
                db.SaveChanges();
            }

            ViewBag.Supporters = new SelectList(db.users.Where(x => x.Role == "admin" || x.Role == "supporter"), "UserID", "UserName");
            ViewBag.CountryID = new SelectList(db.countries, "CountryID", "Code", project.CountryID);
            ViewBag.UserID = new SelectList(db.users.Where(x => x.Role == "admin" || x.Role == "publisher"), "UserID", "UserName", project.UserID);
            ViewBag.Tags = db.tags;
            return View(project);
        }

        //
        // GET: /Admin/Projects/Delete/5
 
        public ActionResult Delete(int id)
        {
            project project = db.projects.Single(p => p.ProjectID == id);
            return View(project);
        }

        //
        // POST: /Admin/Projects/Delete/5

        [HttpPost, ActionName("Delete")]
        public ActionResult DeleteConfirmed(int id)
        {            
            project project = db.projects.Single(p => p.ProjectID == id);
            db.projects.DeleteObject(project);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        //////////////
        //TAGS////////
        //////////////
        public ActionResult TagRemove(int id, int projectid)
        {
            project project = db.projects.Single(p => p.ProjectID == projectid);
            tag tag = db.tags.Single(x => x.TagID == id);
            project.tags.Remove(tag);
            db.SaveChanges();

            return RedirectToAction("Edit","Projects",new {id = projectid});
        }

        [HttpPost]
        public ActionResult TagAdd(string tagname, int projectid) {
            var name = tagname.ToLower().Trim();
            name = Regex.Replace(name, @"[^a-zA-Z]", string.Empty);

            if(string.IsNullOrEmpty(name)) {
                return RedirectToAction("Edit", "Projects", new { id = projectid });
            }

            tag tag = db.tags.FirstOrDefault(x => x.Name.ToLower() == name);

            if(tag == null) {
                tag = new tag(){Name = name};
                db.tags.AddObject(tag);
                //db.SaveChanges();
            }

            project project = db.projects.Single(p => p.ProjectID == projectid);
            if(!project.tags.Contains(tag)) {
                project.tags.Add(tag);
                db.SaveChanges();
            }


            return RedirectToAction("Edit","Projects",new {id = projectid});
        }

        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }
    }
}