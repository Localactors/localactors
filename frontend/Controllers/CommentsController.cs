using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Localactors.entities;

namespace Localactors.webapp.Controllers
{
    public class CommentsController : ControllerBase
    {

        //
        // GET: /Comments/

        public ViewResult Index()
        {
            var update_comment = db.update_comment.Include("update").Include("user");
            return View(update_comment.ToList());
        }

        //
        // GET: /Comments/Details/5

        public ViewResult Details(int id)
        {
            update_comment update_comment = db.update_comment.Single(u => u.CommentID == id);
            return View(update_comment);
        }

        //
        // GET: /Comments/Create

        public ActionResult Create()
        {
            ViewBag.UpdateID = new SelectList(db.updates, "UpdateID", "Title");
            ViewBag.UserID = new SelectList(db.users, "UserID", "Role");
            return View();
        } 

        //
        // POST: /Comments/Create

        [HttpPost]
        public ActionResult Create(update_comment update_comment)
        {
            if (ModelState.IsValid)
            {
                db.update_comment.AddObject(update_comment);
                db.SaveChanges();
                return RedirectToAction("Index");  
            }

            ViewBag.UpdateID = new SelectList(db.updates, "UpdateID", "Title", update_comment.UpdateID);
            ViewBag.UserID = new SelectList(db.users, "UserID", "Role", update_comment.UserID);
            return View(update_comment);
        }
        
        //
        // GET: /Comments/Edit/5
 
        public ActionResult Edit(int id)
        {
            update_comment update_comment = db.update_comment.Single(u => u.CommentID == id);
            ViewBag.UpdateID = new SelectList(db.updates, "UpdateID", "Title", update_comment.UpdateID);
            ViewBag.UserID = new SelectList(db.users, "UserID", "Role", update_comment.UserID);
            return View(update_comment);
        }

        //
        // POST: /Comments/Edit/5

        [HttpPost]
        public ActionResult Edit(update_comment update_comment)
        {
            if (ModelState.IsValid)
            {
                db.update_comment.Attach(update_comment);
                db.ObjectStateManager.ChangeObjectState(update_comment, EntityState.Modified);
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.UpdateID = new SelectList(db.updates, "UpdateID", "Title", update_comment.UpdateID);
            ViewBag.UserID = new SelectList(db.users, "UserID", "Role", update_comment.UserID);
            return View(update_comment);
        }

        //
        // GET: /Comments/Delete/5
 
        public ActionResult Delete(int id)
        {
            update_comment update_comment = db.update_comment.Single(u => u.CommentID == id);
            return View(update_comment);
        }

        //
        // POST: /Comments/Delete/5

        [HttpPost, ActionName("Delete")]
        public ActionResult DeleteConfirmed(int id)
        {            
            update_comment update_comment = db.update_comment.Single(u => u.CommentID == id);
            db.update_comment.DeleteObject(update_comment);
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