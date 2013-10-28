using System.Data;
using System.Linq;
using System.Web.Mvc;
using Localactors.entities;

namespace Localactors.webapp.Areas.Publisher.Controllers
{
    [Authorize(Roles = "publisher,admin")]
    [ValidateInput(false)]
    public class PlansController : Localactors.webapp.Areas.Publisher.ControllerBase
    {
      
     
 
 
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
            var project = db.projects.FirstOrDefault(x => x.ProjectID == project_plan.ProjectID && x.UserID == CurrentUser.UserID);
            if (project == null)
                return RedirectToAction("Index", "Home");

            db.project_plan.DeleteObject(project_plan);
            db.SaveChanges();
            return RedirectToAction("Edit", "Projects", new { id = project_plan.ProjectID }, "plan"); 
        }

        

       
    }
}