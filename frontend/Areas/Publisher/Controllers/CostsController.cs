using System;
using System.Data;
using System.Linq;
using System.Web.Mvc;
using Localactors.entities;

namespace Localactors.webapp.Areas.Publisher.Controllers
{
    [Authorize(Roles = "publisher,admin")]
    [ValidateInput(false)]
    public class CostsController : Localactors.webapp.Areas.Publisher.ControllerBase
    {
     

        
 
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
                return RedirectToAction("Edit", "Projects", new { id = project_cost.ProjectID },"costs");
            }
            ViewBag.ProjectID = new SelectList(db.projects, "ProjectID", "Title", project_cost.ProjectID);
            ViewBag.Project = db.projects.FirstOrDefault(x => x.ProjectID == project_cost.ProjectID);
            return View(project_cost);
        }

 
        public ActionResult Delete(int id, int projectid)
        {
            project_cost project_cost = db.project_cost.Single(p => p.CostID == id);

            var project = db.projects.FirstOrDefault(x => x.ProjectID == project_cost.ProjectID && x.UserID == CurrentUser.UserID);
            if (project == null)
                return RedirectToAction("Index", "Home");


            db.project_cost.DeleteObject(project_cost);
            db.SaveChanges();
            return RedirectToAction("Edit", "Projects", new { id = project_cost.ProjectID }, "costs"); 

        }


    }
}