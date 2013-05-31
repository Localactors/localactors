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
    public class UpdateContentsController :
        ControllerBase
    {
      
        //
        // GET: /Admin/UpdateContents/

        public ViewResult Index()
        {
            var update_content = db.update_content.Include("update").Include("update_content_type");
            return View(update_content.ToList());
        }

        //
        // GET: /Admin/UpdateContents/Details/5

        public ViewResult Details(int id)
        {
            update_content update_content = db.update_content.Single(u => u.ContentID == id);
            return View(update_content);
        }

        //
        // GET: /Admin/UpdateContents/Create

        public ActionResult Create()
        {
            ViewBag.UpdateID = new SelectList(db.updates, "UpdateID", "Title");
            ViewBag.ContentTypeID = new SelectList(db.update_content_type, "ContentTypeID", "ContentTypeName");
            return View();
        } 

        //
        // POST: /Admin/UpdateContents/Create

        [HttpPost]
        public ActionResult Create(update_content update_content)
        {
            if (ModelState.IsValid)
            {
                db.update_content.AddObject(update_content);
                db.SaveChanges();
                return RedirectToAction("Index");  
            }

            ViewBag.UpdateID = new SelectList(db.updates, "UpdateID", "Title", update_content.UpdateID);
            ViewBag.ContentTypeID = new SelectList(db.update_content_type, "ContentTypeID", "ContentTypeName", update_content.ContentTypeID);
            return View(update_content);
        }
        
        //
        // GET: /Admin/UpdateContents/Edit/5
 
        public ActionResult Edit(int id)
        {
            update_content update_content = db.update_content.Single(u => u.ContentID == id);
            ViewBag.UpdateID = new SelectList(db.updates, "UpdateID", "Title", update_content.UpdateID);
            ViewBag.ContentTypeID = new SelectList(db.update_content_type, "ContentTypeID", "ContentTypeName", update_content.ContentTypeID);
            return View(update_content);
        }

        //
        // POST: /Admin/UpdateContents/Edit/5

        [HttpPost]
        public ActionResult Edit(update_content update_content)
        {
            if (ModelState.IsValid)
            {
                db.update_content.Attach(update_content);
                db.ObjectStateManager.ChangeObjectState(update_content, EntityState.Modified);
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.UpdateID = new SelectList(db.updates, "UpdateID", "Title", update_content.UpdateID);
            ViewBag.ContentTypeID = new SelectList(db.update_content_type, "ContentTypeID", "ContentTypeName", update_content.ContentTypeID);
            return View(update_content);
        }

        //
        // GET: /Admin/UpdateContents/Delete/5
 
        public ActionResult Delete(int id)
        {
            update_content update_content = db.update_content.Single(u => u.ContentID == id);
            return View(update_content);
        }

        //
        // POST: /Admin/UpdateContents/Delete/5

        [HttpPost, ActionName("Delete")]
        public ActionResult DeleteConfirmed(int id)
        {            
            update_content update_content = db.update_content.Single(u => u.ContentID == id);
            db.update_content.DeleteObject(update_content);
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