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
    public class CountryController : ControllerBase
    {
     

        //
        // GET: /Admin/Country/

        public ViewResult Index()
        {
            return View(db.countries.AsQueryable().OrderBy("Order").ToList());
        }



        //
        // GET: /Admin/Country/Create

        public ActionResult Create()
        {
            return View();
        } 

        //
        // POST: /Admin/Country/Create

        [HttpPost]
        public ActionResult Create(country country)
        {
            if (ModelState.IsValid)
            {
                db.countries.AddObject(country);
                db.SaveChanges();
                return RedirectToAction("Index");  
            }

            return View(country);
        }
        
        //
        // GET: /Admin/Country/Edit/5
 
        public ActionResult Edit(int id)
        {
            country country = db.countries.Single(c => c.CountryID == id);
            return View(country);
        }

        //
        // POST: /Admin/Country/Edit/5

        [HttpPost]
        public ActionResult Edit(country country)
        {
            if (ModelState.IsValid)
            {
                db.countries.Attach(country);
                db.ObjectStateManager.ChangeObjectState(country, EntityState.Modified);
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(country);
        }

        //
        // GET: /Admin/Country/Delete/5
 
        public ActionResult Delete(int id)
        {
            country country = db.countries.Single(c => c.CountryID == id);
            return View(country);
        }

        //
        // POST: /Admin/Country/Delete/5

        [HttpPost, ActionName("Delete")]
        public ActionResult DeleteConfirmed(int id)
        {            
            country country = db.countries.Single(c => c.CountryID == id);
            db.countries.DeleteObject(country);
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