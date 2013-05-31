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
    public class UpdateContentTypesController : ControllerBase
    {
       
        //
        // GET: /Admin/UpdateContentTypes/

        public ViewResult Index()
        {
            return View(db.update_content_type.ToList());
        }

        //
        // GET: /Admin/UpdateContentTypes/Details/5

        public ViewResult Details(int id)
        {
            update_content_type update_content_type = db.update_content_type.Single(u => u.ContentTypeID == id);
            return View(update_content_type);
        }

        //
        // GET: /Admin/UpdateContentTypes/Create

        public ActionResult Create()
        {
            return View();
        } 

        //
        // POST: /Admin/UpdateContentTypes/Create

        [HttpPost]
        public ActionResult Create(update_content_type update_content_type)
        {
            if (ModelState.IsValid)
            {
                db.update_content_type.AddObject(update_content_type);
                db.SaveChanges();
                return RedirectToAction("Index");  
            }

            return View(update_content_type);
        }
        
        //
        // GET: /Admin/UpdateContentTypes/Edit/5
 
        public ActionResult Edit(int id)
        {
            update_content_type update_content_type = db.update_content_type.Single(u => u.ContentTypeID == id);
            return View(update_content_type);
        }

        //
        // POST: /Admin/UpdateContentTypes/Edit/5

        [HttpPost]
        public ActionResult Edit(update_content_type update_content_type)
        {
            if (ModelState.IsValid)
            {
                db.update_content_type.Attach(update_content_type);
                db.ObjectStateManager.ChangeObjectState(update_content_type, EntityState.Modified);
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(update_content_type);
        }

        //
        // GET: /Admin/UpdateContentTypes/Delete/5
 
        public ActionResult Delete(int id)
        {
            update_content_type update_content_type = db.update_content_type.Single(u => u.ContentTypeID == id);
            return View(update_content_type);
        }

        //
        // POST: /Admin/UpdateContentTypes/Delete/5

        [HttpPost, ActionName("Delete")]
        public ActionResult DeleteConfirmed(int id)
        {            
            update_content_type update_content_type = db.update_content_type.Single(u => u.ContentTypeID == id);
            db.update_content_type.DeleteObject(update_content_type);
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