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
    [ValidateInput(false)]
    public class DonationsController : ControllerBase
    {

        //
        // GET: /Admin/Donations/

        public ViewResult Index(int projectid = 0)
        {
            var donations = db.donations.Include("project").Include("user");
      
            ViewBag.Project = db.projects.FirstOrDefault(x => x.ProjectID == projectid);
            return View(donations.ToList());
        }


        //
        // GET: /Admin/Donations/Create

        public ActionResult Create(int projectid = 0)
        {
            ViewBag.Project = db.projects.FirstOrDefault(x => x.ProjectID == projectid);
            ViewBag.ProjectID = new SelectList(db.projects, "ProjectID", "Title");
            ViewBag.UserID = new SelectList(db.users, "UserID", "Role");
            return View(new donation { ProjectID = projectid });
        }

        //
        // POST: /Admin/Donations/Create

        [HttpPost]
        public ActionResult Create(donation donation, int projectid = 0)
        {
            if (ModelState.IsValid)
            {
                db.donations.AddObject(donation);
                db.SaveChanges();

                //comes from project
                if (projectid > 0)
                {
                    return RedirectToAction("Edit", "Projects", new { id = projectid });
                }


                return RedirectToAction("Index");
            }

            ViewBag.Project = db.projects.FirstOrDefault(x => x.ProjectID == donation.ProjectID);
            ViewBag.ProjectID = new SelectList(db.projects, "ProjectID", "Title");
            ViewBag.UserID = new SelectList(db.users, "UserID", "Role", donation.UserID);
            return View(donation);
        }

        //
        // GET: /Admin/Donations/Edit/5

        public ActionResult Edit(int id)
        {
            donation donation = db.donations.Single(d => d.InvestmentID == id);
            ViewBag.Project = db.projects.FirstOrDefault(x => x.ProjectID == donation.ProjectID);
            ViewBag.ProjectID = new SelectList(db.projects, "ProjectID", "Title", donation.ProjectID);
            ViewBag.UserID = new SelectList(db.users, "UserID", "Role", donation.UserID);
            return View(donation);
        }

        //
        // POST: /Admin/Donations/Edit/5

        [HttpPost]
        public ActionResult Edit(donation donation)
        {
            if (ModelState.IsValid)
            {
                db.donations.Attach(donation);
                db.ObjectStateManager.ChangeObjectState(donation, EntityState.Modified);
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.Project = db.projects.FirstOrDefault(x => x.ProjectID == donation.ProjectID);
            ViewBag.ProjectID = new SelectList(db.projects, "ProjectID", "Title", donation.ProjectID);
            ViewBag.UserID = new SelectList(db.users, "UserID", "Role", donation.UserID);
            return View(donation);
        }

        //
        // GET: /Admin/Donations/Delete/5

        public ActionResult Delete(int id)
        {
            donation donation = db.donations.Single(d => d.InvestmentID == id);
            ViewBag.Project = db.projects.FirstOrDefault(x => x.ProjectID == donation.ProjectID);
            return View(donation);
        }

        //
        // POST: /Admin/Donations/Delete/5

        [HttpPost, ActionName("Delete")]
        public ActionResult DeleteConfirmed(int id, int projectid = 0)
        {
            donation donation = db.donations.Single(d => d.InvestmentID == id);
            db.donations.DeleteObject(donation);
            db.SaveChanges();

            //comes from project
            if (projectid > 0)
            {
                return RedirectToAction("Edit", "Projects", new { id = projectid });
            }

            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }
    }
}