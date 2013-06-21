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
    public class LogsController : ControllerBase
    {
      
        //
        // GET: /Admin/Logs/

        public ViewResult Index()
        {
            return View(db.transaction_dump.ToList());
        }

        //
        // GET: /Admin/Logs/Details/5

        public ViewResult Details(int id)
        {
            transaction_dump transaction_dump = db.transaction_dump.Single(t => t.DumpID == id);
            return View(transaction_dump);
        }

       

        //
        // GET: /Admin/Logs/Delete/5
 
        public ActionResult Delete(int id)
        {
            transaction_dump transaction_dump = db.transaction_dump.Single(t => t.DumpID == id);
            db.transaction_dump.DeleteObject(transaction_dump);
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