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
    public class CostsController : ControllerBase
    {
     


  

        //
        // GET: /Admin/Costs/Create

        public ActionResult Create(int projectid =0)
        {
            ViewBag.ProjectID = new SelectList(db.projects, "ProjectID", "Title");
            ViewBag.Project = db.projects.FirstOrDefault(x => x.ProjectID == projectid);
            return View(new project_cost(){Date = DateTime.Now,ProjectID = projectid});
        } 

        //
        // POST: /Admin/Costs/Create

        [HttpPost]
        public ActionResult Create(project_cost project_cost)
        {
            if (ModelState.IsValid) {

                if (db.project_cost.Any(x => x.ProjectID == project_cost.ProjectID))
                {
                    int order = db.project_plan.Where(x => x.ProjectID == project_cost.ProjectID).Max(x => x.Order);
                    order = order + 1;
                    project_cost.Order = order;
                }
                else
                {
                    project_cost.Order = 20;
                }

                project_cost.Date = DateTime.Now;
                db.project_cost.AddObject(project_cost);
                db.SaveChanges();
                return RedirectToAction("Edit", "Projects", new { id = project_cost.ProjectID });  
            }

            ViewBag.ProjectID = new SelectList(db.projects, "ProjectID", "Title", project_cost.ProjectID);
            ViewBag.Project = db.projects.FirstOrDefault(x => x.ProjectID == project_cost.ProjectID);
            return View(project_cost);
        }
        
 
        public ActionResult Edit(int id)
        {
            project_cost project_cost = db.project_cost.Single(p => p.CostID == id);
            ViewBag.ProjectID = new SelectList(db.projects, "ProjectID", "Title", project_cost.ProjectID);
            ViewBag.Project = db.projects.FirstOrDefault(x => x.ProjectID == project_cost.ProjectID);
            return View(project_cost);
        }


        [HttpPost]
        public ActionResult Edit(project_cost project_cost)
        {
            if (ModelState.IsValid)
            {
                project_cost.Date = DateTime.Now;
                db.project_cost.Attach(project_cost);
                db.ObjectStateManager.ChangeObjectState(project_cost, EntityState.Modified);
                db.SaveChanges();
                return RedirectToAction("Edit", "Projects", new { id = project_cost.ProjectID });
            }
            ViewBag.ProjectID = new SelectList(db.projects, "ProjectID", "Title", project_cost.ProjectID);
            ViewBag.Project = db.projects.FirstOrDefault(x => x.ProjectID == project_cost.ProjectID);
            return View(project_cost);
        }

 
        public ActionResult Delete(int id, int projectid)
        {
            project_cost project_cost = db.project_cost.Single(p => p.CostID == id);
            ViewBag.Project = db.projects.FirstOrDefault(x => x.ProjectID == project_cost.ProjectID);
            return View(project_cost);
        }


        [HttpPost, ActionName("Delete")]
        public ActionResult DeleteConfirmed(int id, int projectid)
        {            
            project_cost project_cost = db.project_cost.Single(p => p.CostID == id);
            db.project_cost.DeleteObject(project_cost);
            db.SaveChanges();

            //comes from project
            if (projectid > 0)
            {
                return RedirectToAction("Edit", "Projects", new { id = projectid });
            }

            return RedirectToAction("Edit", "Projects", new { id = project_cost.ProjectID });
        }

        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }
    }
}