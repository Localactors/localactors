using System.Data;
using System.Linq;
using System.Web.Mvc;
using Localactors.entities;

namespace Localactors.webapp.Areas.Publisher.Controllers
{
    [Authorize(Roles = "publisher,admin")]
    [ValidateInput(false)]
    public class AchievementsController : Localactors.webapp.Areas.Publisher.ControllerBase
    {

      

 
      
        
        
        //
        // GET: /Admin/Achievements/Edit/5
 
        public ActionResult Edit(int id)
        {
            achievement achievement = db.achievements.Single(a => a.AchievementID == id);
            var project = db.projects.FirstOrDefault(x => x.ProjectID == achievement.ProjectID && x.UserID == CurrentUser.UserID);
            if (project == null)
                return RedirectToAction("Index", "Home");

            ViewBag.Project = db.projects.FirstOrDefault(x => x.ProjectID == achievement.ProjectID);
            return View(achievement);
        }

        //
        // POST: /Admin/Achievements/Edit/5

        [HttpPost]
        public ActionResult Edit(achievement achievement)
        {
            var project = db.projects.FirstOrDefault(x => x.ProjectID == achievement.ProjectID && x.UserID == CurrentUser.UserID);
            if (project == null)
                return RedirectToAction("Index", "Home");

            if (ModelState.IsValid)
            {
                
                db.achievements.Attach(achievement);
                db.ObjectStateManager.ChangeObjectState(achievement, EntityState.Modified);
                db.SaveChanges();
                return RedirectToAction("Edit", "Projects", new { id = achievement.ProjectID }, "achievements"); 
            }
            ViewBag.Project = db.projects.FirstOrDefault(x => x.ProjectID == achievement.ProjectID);
            return View(achievement);
        }

        //
        // GET: /Admin/Achievements/Delete/5
 
        public ActionResult Delete(int id)
        {
            achievement achievement = db.achievements.Single(a => a.AchievementID == id);
            var project = db.projects.FirstOrDefault(x => x.ProjectID == achievement.ProjectID && x.UserID == CurrentUser.UserID);
            if (project == null)
                return RedirectToAction("Index", "Home");

            db.achievements.DeleteObject(achievement);
            db.SaveChanges();
            return RedirectToAction("Edit", "Projects", new { id = achievement.ProjectID }, "achievements"); 
        }


      
    }
}