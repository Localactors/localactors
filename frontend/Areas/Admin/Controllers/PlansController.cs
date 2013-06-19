using System.Data;
using System.Linq;
using System.Web.Mvc;
using Localactors.entities;

namespace Localactors.webapp.Areas.Admin.Controllers
{
    [Authorize(Roles = "admin")]
    [ValidateInput(false)]
    public class PlansController : ControllerBase
    {
      
        public ActionResult Create(int projectid=0)
        {
            ViewBag.ProjectID = new SelectList(db.projects, "ProjectID", "Title");
            ViewBag.Project = db.projects.FirstOrDefault(x => x.ProjectID == projectid);
            return View(new project_plan(){ProjectID = projectid});
        } 


        [HttpPost]
        public ActionResult Create(project_plan project_plan)
        {
            if (ModelState.IsValid) {
                if (db.project_plan.Any(x => x.ProjectID == project_plan.ProjectID)) {
                    int order = db.project_plan.Where(x => x.ProjectID == project_plan.ProjectID).Max(x => x.Order);
                    order = order + 1;
                    project_plan.Order = order;
                }
                else {
                    project_plan.Order = 20;
                }
                db.project_plan.AddObject(project_plan);
                db.SaveChanges();
                return RedirectToAction("Edit", "Projects", new { id = project_plan.ProjectID },"plan");  
            }

            ViewBag.ProjectID = new SelectList(db.projects, "ProjectID", "Title", project_plan.ProjectID);
            ViewBag.Project = db.projects.FirstOrDefault(x => x.ProjectID == project_plan.ProjectID);
            return View(project_plan);
        }
        
 
 
        public ActionResult Edit(int id)
        {
            project_plan project_plan = db.project_plan.Single(p => p.PlanID == id);
            ViewBag.ProjectID = new SelectList(db.projects, "ProjectID", "Title", project_plan.ProjectID);
            ViewBag.Project = db.projects.FirstOrDefault(x => x.ProjectID == project_plan.ProjectID);
            return View(project_plan);
        }

        //
        // POST: /Admin/Plans/Edit/5

        [HttpPost]
        public ActionResult Edit(project_plan project_plan)
        {
            if (ModelState.IsValid)
            {
                db.project_plan.Attach(project_plan);
                db.ObjectStateManager.ChangeObjectState(project_plan, EntityState.Modified);
                db.SaveChanges();
                return RedirectToAction("Edit", "Projects", new { id = project_plan.ProjectID },"plan"); 
            }
            ViewBag.ProjectID = new SelectList(db.projects, "ProjectID", "Title", project_plan.ProjectID);
            ViewBag.Project = db.projects.FirstOrDefault(x => x.ProjectID == project_plan.ProjectID);
            return View(project_plan);
        }

        //
        // GET: /Admin/Plans/Delete/5
 
        public ActionResult Delete(int id)
        {
            project_plan project_plan = db.project_plan.Single(p => p.PlanID == id);
            ViewBag.Project = db.projects.FirstOrDefault(x => x.ProjectID == project_plan.ProjectID);
            return View(project_plan);
        }

        //
        // POST: /Admin/Plans/Delete/5

        [HttpPost, ActionName("Delete")]
        public ActionResult DeleteConfirmed(int id)
        {            
            project_plan project_plan = db.project_plan.Single(p => p.PlanID == id);
            db.project_plan.DeleteObject(project_plan);
            db.SaveChanges();
            return RedirectToAction("Edit", "Projects", new { id = project_plan.ProjectID },"plan"); 
        }

        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }
    }
}