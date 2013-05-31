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
    public class TagController : ControllerBase
    {
   
        // GET: /Admin/Tag/

        public ViewResult Index()
        {
            return View(db.tags.ToList());
        }

        //
        // GET: /Admin/Tag/Details/5

        public ViewResult Details(int id)
        {
            tag tag = db.tags.Single(t => t.TagID == id);
            return View(tag);
        }

        //
        // GET: /Admin/Tag/Create

        public ActionResult Create()
        {
            return View();
        } 

        //
        // POST: /Admin/Tag/Create

        [HttpPost]
        public ActionResult Create(tag tag)
        {
            if (ModelState.IsValid)
            {
                db.tags.AddObject(tag);
                db.SaveChanges();
                return RedirectToAction("Index");  
            }

            return View(tag);
        }
        
        //
        // GET: /Admin/Tag/Edit/5
 
        public ActionResult Edit(int id)
        {
            tag tag = db.tags.Single(t => t.TagID == id);
            return View(tag);
        }

        //
        // POST: /Admin/Tag/Edit/5

        [HttpPost]
        public ActionResult Edit(tag tag)
        {
            if (ModelState.IsValid)
            {
                db.tags.Attach(tag);
                db.ObjectStateManager.ChangeObjectState(tag, EntityState.Modified);
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(tag);
        }

        //
        // GET: /Admin/Tag/Delete/5
 
        public ActionResult Delete(int id)
        {
            tag tag = db.tags.Single(t => t.TagID == id);
            return View(tag);
        }

        //
        // POST: /Admin/Tag/Delete/5

        [HttpPost, ActionName("Delete")]
        public ActionResult DeleteConfirmed(int id)
        {            
            tag tag = db.tags.Single(t => t.TagID == id);
            db.tags.DeleteObject(tag);
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