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
    public class SupporterController : ControllerBase
    {


        //
        // GET: /Supporter/

        public ViewResult Index()
        {
            var users = db.users.Include("country");
            return View(users.ToList());
        }

        //
        // GET: /Supporter/Details/5

        public ViewResult Details(int id)
        {
            user user = db.users.Single(u => u.UserID == id);
            return View(user);
        }

        //
        // GET: /Supporter/Create

        public ActionResult Create()
        {
            ViewBag.CountryID = new SelectList(db.countries, "CountryID", "Code");
            return View();
        } 

        //
        // POST: /Supporter/Create

        [HttpPost]
        public ActionResult Create(user user)
        {
            if (ModelState.IsValid)
            {
                db.users.AddObject(user);
                db.SaveChanges();
                return RedirectToAction("Index");  
            }

            ViewBag.CountryID = new SelectList(db.countries, "CountryID", "Code", user.CountryID);
            return View(user);
        }
        
        //
        // GET: /Supporter/Edit/5
 
        public ActionResult Edit(int id)
        {
            user user = db.users.Single(u => u.UserID == id);
            ViewBag.CountryID = new SelectList(db.countries, "CountryID", "Code", user.CountryID);
            return View(user);
        }

        //
        // POST: /Supporter/Edit/5

        [HttpPost]
        public ActionResult Edit(user user)
        {
            if (ModelState.IsValid)
            {
                db.users.Attach(user);
                db.ObjectStateManager.ChangeObjectState(user, EntityState.Modified);
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.CountryID = new SelectList(db.countries, "CountryID", "Code", user.CountryID);
            return View(user);
        }

        //
        // GET: /Supporter/Delete/5
 
        public ActionResult Delete(int id)
        {
            user user = db.users.Single(u => u.UserID == id);
            return View(user);
        }

        //
        // POST: /Supporter/Delete/5

        [HttpPost, ActionName("Delete")]
        public ActionResult DeleteConfirmed(int id)
        {            
            user user = db.users.Single(u => u.UserID == id);
            db.users.DeleteObject(user);
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