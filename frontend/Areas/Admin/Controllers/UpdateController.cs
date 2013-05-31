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
    public class UpdateController : ControllerBase
    {

        //
        // GET: /Admin/Update/

        public ActionResult Index(int projectid = 0)
        {
            if (projectid <= 0) return RedirectToAction("Index", "Projects");

            var updates = db.updates.Include("project").Include("update_content").Where(x => x.ProjectID == projectid);

            ViewBag.Projectid = projectid;
            ViewBag.Project = db.projects.FirstOrDefault(x => x.ProjectID == projectid);
            return View(updates.OrderByDescending("Date").ToList());
        }

        //
        // GET: /Admin/Update/Details/5

     

        //
        // GET: /Admin/Update/Create

        public ActionResult Create(int projectid = 0)
        {
            if (projectid <= 0) return RedirectToAction("Index", "Projects");

            ViewBag.ProjectID = new SelectList(db.projects, "ProjectID", "Title");
            ViewBag.Project = db.projects.FirstOrDefault(x => x.ProjectID == projectid);
            return View(new update() { ProjectID = projectid, Date = DateTime.Now,UserID = CurrentUser.UserID, DateCreated = DateTime.Now});
        }

        //
        // POST: /Admin/Update/Create

        [HttpPost]
        public ActionResult Create(update update, int projectid = 0)
        {
            update.UserID = CurrentUser.UserID;
            update.DateCreated = DateTime.Now;
            if (ModelState.IsValid)
            {
                db.updates.AddObject(update);
                db.SaveChanges();

                return RedirectToAction("Edit", "Update", new { id = update.UpdateID });
            }

            ViewBag.ProjectID = new SelectList(db.projects, "ProjectID", "Title", update.ProjectID);
            ViewBag.Project = db.projects.FirstOrDefault(x => x.ProjectID == projectid);
            return View(update);
        }

        //
        // GET: /Admin/Update/Edit/5

        public ActionResult Edit(int id)
        {
            update update = db.updates.Single(u => u.UpdateID == id);
            ViewBag.ProjectID = new SelectList(db.projects, "ProjectID", "Title", update.ProjectID);
            ViewBag.Project = db.projects.FirstOrDefault(x => x.ProjectID == update.ProjectID);
            return View(update);
        }

        //
        // POST: /Admin/Update/Edit/5

        [HttpPost]
        public ActionResult Edit(update update)
        {
            if (ModelState.IsValid)
            {
                db.updates.Attach(update);
                db.ObjectStateManager.ChangeObjectState(update, EntityState.Modified);
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.Project = db.projects.FirstOrDefault(x => x.ProjectID == update.ProjectID);
            ViewBag.ProjectID = new SelectList(db.projects, "ProjectID", "Title", update.ProjectID);
            return View(update);
        }

        //
        // GET: /Admin/Update/Delete/5

        public ActionResult Delete(int id)
        {
            update update = db.updates.Single(u => u.UpdateID == id);
            ViewBag.Project = db.projects.FirstOrDefault(x => x.ProjectID == update.ProjectID);
            return View(update);
        }

        //
        // POST: /Admin/Update/Delete/5

        [HttpPost, ActionName("Delete")]
        public ActionResult DeleteConfirmed(int id, int projectid =0) {
            var cts = db.update_content.Where(x => x.UpdateID == id);
            var cms = db.update_comment.Where(x => x.UpdateID == id);

            foreach (var content in cts) {
                db.update_content.DeleteObject(content);
            }

            foreach (var comment in cms)
            {
                db.update_comment.DeleteObject(comment);
            }

            update update = db.updates.Single(u => u.UpdateID == id);
            db.updates.DeleteObject(update);
            db.SaveChanges();

            //comes from project
            if (projectid >0) {
                return RedirectToAction("Edit","Projects",new{id = projectid});
            }

            return RedirectToAction("Index");
        }


        //*****************************************************************************//
        //*****************************************************************************//
        //CONTENTS
        //*****************************************************************************//
        //*****************************************************************************//

        [ValidateInput(false)]
        public ActionResult ContentCreate(update_content update_content) {
            int order = 100;
            if (db.update_content.Any(x => x.UpdateID == update_content.UpdateID)) {
                order = db.update_content.Where(x => x.UpdateID == update_content.UpdateID).Max(x => x.Order);
                order = order + 1;
            }

            if (update_content.Date == null) update_content.Date = DateTime.Now;
            update_content.DateCreated = DateTime.Now;
            update_content.UserID = CurrentUser.UserID;
            update_content.Order = order;
            if (ModelState.IsValid)
            {
                db.update_content.AddObject(update_content);
                db.SaveChanges();
                return RedirectToAction("Edit", "Update", new { id = update_content.UpdateID });
            }

            return RedirectToAction("Edit", "Update", new { id = update_content.UpdateID });
        }

        public ActionResult ContentDelete(int id, int updateid)
        {
            var content = db.update_content.FirstOrDefault(x => x.ContentID == id);
            db.update_content.DeleteObject(content);
            db.SaveChanges();
            return RedirectToAction("Edit", "Update", new { id = updateid });
        }

        public ActionResult ContentUp(int id, int updateid)
        {
            //get the data
            var content = db.update_content.FirstOrDefault(x => x.ContentID == id);
            var contents = db.update_content.Where(x => x.UpdateID == updateid).OrderByDescending("Order");

            //search for the swappable content
            update_content othercontent = null;
            foreach (update_content updateContent in contents) {
                if (updateContent.ContentID == id) {
                    break;
                }
                othercontent = updateContent;
            }

            //swap
            if (othercontent != null && othercontent.ContentID != content.ContentID) {
                int oldorder = content.Order;
                content.Order = othercontent.Order;
                othercontent.Order = oldorder;
                db.SaveChanges();
            }

            return RedirectToAction("Edit", "Update", new { id = updateid });
        }
        public ActionResult ContentDown(int id, int updateid)
        {


            //get the data 
            var content = db.update_content.FirstOrDefault(x => x.ContentID == id);
            //(reverse the ordering , to sort in the right direction)
            var contents = db.update_content.Where(x => x.UpdateID == updateid).OrderBy("Order");

            //search for the swappable content
            update_content othercontent = null;
            foreach (update_content updateContent in contents)
            {
                if (updateContent.ContentID == id)
                {
                    break;
                }
                othercontent = updateContent;
            }

            //swap
            if (othercontent != null && othercontent.ContentID != content.ContentID)
            {
                int oldorder = content.Order;
                content.Order = othercontent.Order;
                othercontent.Order = oldorder;
                db.SaveChanges();
            }

            return RedirectToAction("Edit", "Update", new { id = updateid });
        }




        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }
    }
}