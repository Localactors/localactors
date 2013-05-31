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
    public class UserController : ControllerBase
    {
        

        //
        // GET: /Admin/User/

        public ViewResult Index(string role = null)
        {
            var users = db.users.Include("country").AsQueryable();
            if (role != null)
                users = users.Where(x => x.Role == role);

            return View(users.ToList());
        }


        //
        // GET: /Admin/User/Create

        public ActionResult Create()
        {
            ViewBag.CountryID = new SelectList(db.countries, "CountryID", "Code");
            ViewBag.Role = new SelectList(db.user_roles, "RoleName", "RoleName");
            return View(new user(){DateJoined = DateTime.Now,DateLastLogin = DateTime.Now,Confirmed = true,Enabled = true,Privacy = true});
        } 

        //
        // POST: /Admin/User/Create

        [HttpPost]
        public ActionResult Create(user user)
        {
            user.DateJoined = DateTime.Now;
            user.DateLastLogin = DateTime.Now;

            if (ModelState.IsValid) {
                db.users.AddObject(user);
                db.SaveChanges();
                return RedirectToAction("Index");  
            }

            ViewBag.CountryID = new SelectList(db.countries, "CountryID", "Code", user.CountryID);
            ViewBag.Role = new SelectList(db.user_roles, "RoleName", "RoleName");
            return View(user);
        }
        
        //
        // GET: /Admin/User/Edit/5
 
        public ActionResult Edit(int id)
        {
            user user = db.users.Single(u => u.UserID == id);
            ViewBag.CountryID = new SelectList(db.countries, "CountryID", "Code", user.CountryID);
            ViewBag.Role = new SelectList(db.user_roles, "RoleName", "RoleName",user.Role);
            return View(user);
        }

        //
        // POST: /Admin/User/Edit/5

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
            ViewBag.Role = new SelectList(db.user_roles, "RoleName", "RoleName", user.Role);
            return View(user);
        }

        //
        // GET: /Admin/User/Delete/5
 
        public ActionResult Delete(int id)
        {
            user user = db.users.Single(u => u.UserID == id);
            return View(user);
        }

        //
        // POST: /Admin/User/Delete/5

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