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
    public class TransactionsController : ControllerBase
    {
    
        public ViewResult Index(int donationid = 0)
        {
            var transactions = db.transactions.Include("donation").AsQueryable();
            if(donationid>0) {
                transactions = transactions.Where(x => x.DonationID == donationid);
            }

            if (donationid > 0) {
                ViewBag.Donation = db.donations.FirstOrDefault(x => x.InvestmentID == donationid);
                ViewBag.Project = ViewBag.Donation.Project;
            }
            return View(transactions.ToList());
        }

     
        public ActionResult Edit(int id)
        {
            transaction transaction = db.transactions.Single(t => t.TransactionID == id);

            ViewBag.DonationID = new SelectList(db.donations, "InvestmentID", "Description", transaction.DonationID);
            var donation = db.donations.FirstOrDefault(x => x.InvestmentID == transaction.DonationID);
            ViewBag.Donation = donation;
            ViewBag.Project = donation.project;
            return View(transaction);
        }


        [HttpPost]
        public ActionResult Edit(transaction transaction)
        {
            if (ModelState.IsValid)
            {
                db.transactions.Attach(transaction);
                db.ObjectStateManager.ChangeObjectState(transaction, EntityState.Modified);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.DonationID = new SelectList(db.donations, "InvestmentID", "Description", transaction.DonationID);
            var donation = db.donations.FirstOrDefault(x => x.InvestmentID == transaction.DonationID);
            ViewBag.Donation = donation;
            ViewBag.Project = donation.project;
            return View(transaction);
        }


        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }
    }
}