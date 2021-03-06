﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;
using Localactors.entities;

namespace Localactors.webapp.Areas.Admin.Controllers
{ 
    public class MailinglistController : ControllerBase
    {


        //
        // GET: /Admin/Mailinglist/

        public ViewResult Index()
        {
            return View(db.mailinglists.ToList());
        }

        public ActionResult Export()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendFormat("{0},{1}\r\n", "Address", "Name");
            foreach (var source in db.mailinglists.ToList()) {
                sb.AppendFormat("{0},{1}\r\n",source.Address,source.Name);
            }
            sb.AppendLine();

            //output
            Response.Clear();
            Response.ContentType = "application/CSV";
            Response.AddHeader("Content-Disposition", "attachment;filename=mailinglist.csv");
            return Content(sb.ToString());
        }

  
        //
        // GET: /Admin/Mailinglist/Create

        public ActionResult Create()
        {
            return View();
        } 

        //
        // POST: /Admin/Mailinglist/Create

        [HttpPost]
        public ActionResult Create(mailinglist mailinglist)
        {
            if (ModelState.IsValid)
            {
                db.mailinglists.AddObject(mailinglist);
                db.SaveChanges();
                return RedirectToAction("Index");  
            }

            return View(mailinglist);
        }
        
        //
        // GET: /Admin/Mailinglist/Edit/5
 
        public ActionResult Edit(int id)
        {
            mailinglist mailinglist = db.mailinglists.Single(m => m.AddressID == id);
            return View(mailinglist);
        }

        //
        // POST: /Admin/Mailinglist/Edit/5

        [HttpPost]
        public ActionResult Edit(mailinglist mailinglist)
        {
            if (ModelState.IsValid)
            {
                db.mailinglists.Attach(mailinglist);
                db.ObjectStateManager.ChangeObjectState(mailinglist, EntityState.Modified);
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(mailinglist);
        }

        //
        // GET: /Admin/Mailinglist/Delete/5
 
        public ActionResult Delete(int id)
        {
            mailinglist mailinglist = db.mailinglists.Single(m => m.AddressID == id);
            return View(mailinglist);
        }

        //
        // POST: /Admin/Mailinglist/Delete/5

        [HttpPost, ActionName("Delete")]
        public ActionResult DeleteConfirmed(int id)
        {            
            mailinglist mailinglist = db.mailinglists.Single(m => m.AddressID == id);
            db.mailinglists.DeleteObject(mailinglist);
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