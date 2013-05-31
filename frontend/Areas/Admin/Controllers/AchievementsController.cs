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
    [Authorize(Roles = "admin")]
    public class AchievementsController : ControllerBase
    {

        //
        // GET: /Admin/Achievements/

        public ViewResult Index(int projectid =0)
        {
            var achievements = db.achievements.Include("project");
            ViewBag.Project = db.projects.FirstOrDefault(x => x.ProjectID == projectid);
            return View(achievements.ToList());
        }

 
        //
        // GET: /Admin/Achievements/Create

        public ActionResult Create(int projectid = 0)
        {
            ViewBag.ProjectID = new SelectList(db.projects, "ProjectID", "Title");

            return View(new achievement { ProjectID = projectid });
        } 

        //
        // POST: /Admin/Achievements/Create

        [HttpPost]
        public ActionResult Create(achievement achievement, int projectid = 0)
        {
            if (ModelState.IsValid)
            {
                db.achievements.AddObject(achievement);
                db.SaveChanges();
                return RedirectToAction("Index");  
            }

            ViewBag.Project = db.projects.FirstOrDefault(x => x.ProjectID == achievement.ProjectID);

            return View(achievement);
        }
        
        //
        // GET: /Admin/Achievements/Edit/5
 
        public ActionResult Edit(int id)
        {
            achievement achievement = db.achievements.Single(a => a.AchievementID == id);
            ViewBag.Project = db.projects.FirstOrDefault(x => x.ProjectID == achievement.ProjectID);
            ViewBag.ProjectID = new SelectList(db.projects, "ProjectID", "Title", achievement.ProjectID);
            return View(achievement);
        }

        //
        // POST: /Admin/Achievements/Edit/5

        [HttpPost]
        public ActionResult Edit(achievement achievement)
        {
            if (ModelState.IsValid)
            {
                db.achievements.Attach(achievement);
                db.ObjectStateManager.ChangeObjectState(achievement, EntityState.Modified);
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.Project = db.projects.FirstOrDefault(x => x.ProjectID == achievement.ProjectID);
            ViewBag.ProjectID = new SelectList(db.projects, "ProjectID", "Title", achievement.ProjectID);
            return View(achievement);
        }

        //
        // GET: /Admin/Achievements/Delete/5
 
        public ActionResult Delete(int id)
        {
            achievement achievement = db.achievements.Single(a => a.AchievementID == id);
            ViewBag.Project = db.projects.FirstOrDefault(x => x.ProjectID == achievement.ProjectID);
            return View(achievement);
        }

        //
        // POST: /Admin/Achievements/Delete/5

        [HttpPost, ActionName("Delete")]
        public ActionResult DeleteConfirmed(int id, int projectid = 0)
        {            
            achievement achievement = db.achievements.Single(a => a.AchievementID == id);
            db.achievements.DeleteObject(achievement);
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