using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.Entity;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Amazon.S3;
using Amazon.S3.Model;
using Localactors.entities;

namespace Localactors.webapp.Areas.Admin.Controllers
{
    [Authorize(Roles = "admin")]
    [ValidateInput(false)]
    public class UpdateController : ControllerBase
    {



        public ActionResult Index(int projectid = 0)
        {
            if (projectid <= 0) return RedirectToAction("Index", "Projects");

            var updates = db.updates.Include("project").Include("update_content").Where(x => x.ProjectID == projectid);

            ViewBag.Projectid = projectid;
            ViewBag.Project = db.projects.FirstOrDefault(x => x.ProjectID == projectid);
            return View(updates.OrderByDescending("Date").ToList());
        }

        

        public ActionResult Create(int projectid = 0)
        {
            if (projectid <= 0) return RedirectToAction("Index", "Projects");

            ViewBag.ProjectID = new SelectList(db.projects, "ProjectID", "Title");
            ViewBag.Project = db.projects.FirstOrDefault(x => x.ProjectID == projectid);
            return View(new update() { ProjectID = projectid, Date = DateTime.Now, UserID = CurrentUser.UserID, DateCreated = DateTime.Now });
        }



        [HttpPost]
        public ActionResult Create(update update, int projectid = 0)
        {
            update.UserID = CurrentUser.UserID;
            update.DateCreated = DateTime.Now;
            if (ModelState.IsValid)
            {
                db.updates.AddObject(update);
                db.SaveChanges();

                return RedirectToAction("Edit", "Update", new { id = update.UpdateID });
            }

            ViewBag.ProjectID = new SelectList(db.projects, "ProjectID", "Title", update.ProjectID);
            ViewBag.Project = db.projects.FirstOrDefault(x => x.ProjectID == projectid);
            return View(update);
        }



        public ActionResult Edit(int id)
        {
            update update = db.updates.Single(u => u.UpdateID == id);
            ViewBag.ProjectID = new SelectList(db.projects, "ProjectID", "Title", update.ProjectID);
            ViewBag.Project = db.projects.FirstOrDefault(x => x.ProjectID == update.ProjectID);
            return View(update);
        }



        [HttpPost]
        public ActionResult Edit(update update)
        {
            if (ModelState.IsValid)
            {
                db.updates.Attach(update);
                db.ObjectStateManager.ChangeObjectState(update, EntityState.Modified);
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.Project = db.projects.FirstOrDefault(x => x.ProjectID == update.ProjectID);
            ViewBag.ProjectID = new SelectList(db.projects, "ProjectID", "Title", update.ProjectID);
            return View(update);
        }



        public ActionResult Delete(int id)
        {
            update update = db.updates.Single(u => u.UpdateID == id);
            ViewBag.Project = db.projects.FirstOrDefault(x => x.ProjectID == update.ProjectID);
            return View(update);
        }

        [HttpPost, ActionName("Delete")]
        public ActionResult DeleteConfirmed(int id, int projectid = 0)
        {
            var cts = db.update_content.Where(x => x.UpdateID == id);
            var cms = db.update_comment.Where(x => x.UpdateID == id);

            foreach (var content in cts)
            {
                db.update_content.DeleteObject(content);
            }

            foreach (var comment in cms)
            {
                db.update_comment.DeleteObject(comment);
            }

            update update = db.updates.Single(u => u.UpdateID == id);
            db.updates.DeleteObject(update);
            db.SaveChanges();

            //comes from project
            if (projectid > 0)
            {
                return RedirectToAction("Edit", "Projects", new { id = projectid },"updates");
            }

            return RedirectToAction("Index");
        }


        //*****************************************************************************//
        //*****************************************************************************//
        //CONTENTS
        //*****************************************************************************//
        //*****************************************************************************//

        [ValidateInput(false)]
        public ActionResult ContentCreate(update_content update_content, int ProjectID = 0)
        {
            //File Upload
            if (Request.Files != null && Request.Files.Count > 0)
            {
                foreach (string keyname in Request.Files)
                {
                    HttpPostedFileBase file = Request.Files[keyname];
                    if (file != null && file.ContentLength > 0 && !string.IsNullOrEmpty(file.FileName))
                    {
                        //file upload
                        string ext = Path.GetExtension(file.FileName).ToLower();
                        if (ext != ".png" && ext != ".jpg" && ext != ".jpeg" && ext != ".swf" && ext != ".fla" && ext!=".mov" && ext!= ".avi")
                        {
                            ModelState.AddModelError(keyname, "Invalid file type");
                            return RedirectToAction("Edit", "Update", new { id = update_content.UpdateID });
                        }

                        try
                        {
                            if (ext == ".png" || ext == ".jpg" || ext == ".jpeg") {
                                using (Image tmp = Image.FromStream(file.InputStream)) {
                                    //resize+crop
                                    int width = int.Parse(ConfigurationManager.AppSettings["Image_Update_Width"]);
                                    int height = int.Parse(ConfigurationManager.AppSettings["Image_Update_Height"]);
                                    string name = getTimestamp() + ".jpg";
                                    string filepath = string.Format("projects/{0}/update_{1}/{2}", ProjectID, update_content.UpdateID, name);
                                    string address = ConfigurationManager.AppSettings["AWSS3BucketUrl"] + filepath;

                                    //send
                                    using (Image resized = tmp.GetResizedImage(width, height, true)) {
                                        var request = new PutObjectRequest().WithBucketName(ConfigurationManager.AppSettings["AWSS3Bucket"]).WithKey(filepath);
                                        using (MemoryStream buffer = new MemoryStream()) {
                                            resized.Save(buffer, ImageHelper.GetJpgEncoder(), ImageHelper.GetJpgEncoderParameters(80));
                                            request.InputStream = buffer;
                                            AmazonS3Client s3Client = new AmazonS3Client();
                                            s3Client.PutObject(request);
                                        }
                                    }

                                    ModelState.Remove(keyname);
                                    ModelState.Add(keyname, new ModelState());
                                    ModelState.SetModelValue(keyname, new ValueProviderResult(address, address, null));
                                    update_content.Media = address;
                                    update_content.Url = address;
                                }
                            }else {
                                //ok, making the new filename
                                string filepath = string.Format("projects/{0}/update_{1}/{2}", ProjectID, update_content.UpdateID, file.FileName);
                                var request = new PutObjectRequest().WithBucketName(ConfigurationManager.AppSettings["AWSS3Bucket"]).WithKey(filepath);
                                request.InputStream = file.InputStream;
                                AmazonS3Client.PutObject(request);
                                string address = ConfigurationManager.AppSettings["AWSS3BucketUrl"] + filepath;

                                ModelState.Remove(keyname);
                                ModelState.Add(keyname, new ModelState());
                                ModelState.SetModelValue(keyname, new ValueProviderResult(address, address, null));
                                update_content.Media = address;
                                update_content.Url = address;
                            }

                        }
                        catch (Exception ex)
                        {
                            ModelState.AddModelError(keyname, "Upload error: " + ex.Message);
                            return RedirectToAction("Edit", "Update", new { id = update_content.UpdateID });
                        }
                    }
                }
            }


            int order = 100;
            if (db.update_content.Any(x => x.UpdateID == update_content.UpdateID))
            {
                order = db.update_content.Where(x => x.UpdateID == update_content.UpdateID).Max(x => x.Order);
                order = order + 1;
            }

            if (update_content.Date == null) update_content.Date = DateTime.Now;
            update_content.DateCreated = DateTime.Now;
            update_content.UserID = CurrentUser.UserID;
            update_content.Order = order;
            if (ModelState.IsValid)
            {
                db.update_content.AddObject(update_content);
                db.SaveChanges();
                return RedirectToAction("Edit", "Update", new { id = update_content.UpdateID });
            }

            return RedirectToAction("Edit", "Update", new { id = update_content.UpdateID });
        }
        [ValidateInput(false)]
        public ActionResult ContentEdit(update_content update_content, int ProjectID = 0)
        {
            //File Upload
            if (Request.Files != null && Request.Files.Count > 0)
            {
                foreach (string keyname in Request.Files)
                {
                    HttpPostedFileBase file = Request.Files[keyname];
                    if (file != null && file.ContentLength > 0 && !string.IsNullOrEmpty(file.FileName))
                    {
                        //file upload
                        string ext = Path.GetExtension(file.FileName).ToLower();
                        if (ext != ".png" && ext != ".jpg" && ext != ".jpeg" && ext != ".swf" && ext != ".fla")
                        {
                            ModelState.AddModelError(keyname, "Invalid file type");
                            return RedirectToAction("Edit", "Update", new { id = update_content.UpdateID });
                        }

                        try
                        {
                            if (ext == ".png" || ext == ".jpg" || ext == ".jpeg")
                            {
                                using (Image tmp = Image.FromStream(file.InputStream))
                                {
                                    //resize+crop
                                    int width = int.Parse(ConfigurationManager.AppSettings["Image_Update_Width"]);
                                    int height = int.Parse(ConfigurationManager.AppSettings["Image_Update_Height"]);
                                    string name = getTimestamp() + ".jpg";
                                    string filepath = string.Format("projects/{0}/update_{1}/{2}", ProjectID, update_content.UpdateID, name);
                                    string address = ConfigurationManager.AppSettings["AWSS3BucketUrl"] + filepath;

                                    //send
                                    using (Image resized = tmp.GetResizedImage(width, height, true))
                                    {
                                        var request = new PutObjectRequest().WithBucketName(ConfigurationManager.AppSettings["AWSS3Bucket"]).WithKey(filepath);
                                        using (MemoryStream buffer = new MemoryStream())
                                        {
                                            resized.Save(buffer, ImageHelper.GetJpgEncoder(), ImageHelper.GetJpgEncoderParameters(80));
                                            request.InputStream = buffer;
                                            AmazonS3Client s3Client = new AmazonS3Client();
                                            s3Client.PutObject(request);
                                        }
                                    }

                                    ModelState.Remove(keyname);
                                    ModelState.Add(keyname, new ModelState());
                                    ModelState.SetModelValue(keyname, new ValueProviderResult(address, address, null));
                                    update_content.Media = address;
                                    update_content.Url = address;
                                }
                            }
                            else
                            {
                                //ok, making the new filename
                                string filepath = string.Format("projects/{0}/update_{1}/{2}", ProjectID, update_content.UpdateID, file.FileName);
                                var request = new PutObjectRequest().WithBucketName(ConfigurationManager.AppSettings["AWSS3Bucket"]).WithKey(filepath);
                                request.InputStream = file.InputStream;
                                AmazonS3Client.PutObject(request);
                                string address = ConfigurationManager.AppSettings["AWSS3BucketUrl"] + filepath;

                                ModelState.Remove(keyname);
                                ModelState.Add(keyname, new ModelState());
                                ModelState.SetModelValue(keyname, new ValueProviderResult(address, address, null));
                                update_content.Media = address;
                                update_content.Url = address;
                            }
                        }
                        catch (Exception ex)
                        {
                            ModelState.AddModelError(keyname, "Upload error: " + ex.Message);
                            return RedirectToAction("Edit", "Update", new { id = update_content.UpdateID });
                        }
                    }
                }
            }

            if (update_content.Date == null) update_content.Date = DateTime.Now;
            update_content.DateCreated = DateTime.Now;
            update_content.UserID = CurrentUser.UserID;

            if (ModelState.IsValid)
            {
                db.update_content.Attach(update_content);
                db.ObjectStateManager.ChangeObjectState(update_content, EntityState.Modified);
                db.SaveChanges();
                return RedirectToAction("Edit", "Update", new { id = update_content.UpdateID });
            }

            return RedirectToAction("Edit", "Update", new { id = update_content.UpdateID });
        }
        public ActionResult ContentDelete(int id, int updateid)
        {
            var content = db.update_content.FirstOrDefault(x => x.ContentID == id);
            db.update_content.DeleteObject(content);
            db.SaveChanges();
            return RedirectToAction("Edit", "Update", new { id = updateid });
        }

        public ActionResult ContentUp(int id, int updateid)
        {
            //get the data
            var content = db.update_content.FirstOrDefault(x => x.ContentID == id);
            var contents = db.update_content.Where(x => x.UpdateID == updateid).OrderByDescending("Order");

            //search for the swappable content
            update_content othercontent = null;
            foreach (update_content updateContent in contents)
            {
                if (updateContent.ContentID == id)
                {
                    break;
                }
                othercontent = updateContent;
            }

            //swap
            if (othercontent != null && othercontent.ContentID != content.ContentID)
            {
                int oldorder = content.Order;
                content.Order = othercontent.Order;
                othercontent.Order = oldorder;
                db.SaveChanges();
            }

            return RedirectToAction("Edit", "Update", new { id = updateid });
        }
        public ActionResult ContentDown(int id, int updateid)
        {


            //get the data 
            var content = db.update_content.FirstOrDefault(x => x.ContentID == id);
            //(reverse the ordering , to sort in the right direction)
            var contents = db.update_content.Where(x => x.UpdateID == updateid).OrderBy("Order");

            //search for the swappable content
            update_content othercontent = null;
            foreach (update_content updateContent in contents)
            {
                if (updateContent.ContentID == id)
                {
                    break;
                }
                othercontent = updateContent;
            }

            //swap
            if (othercontent != null && othercontent.ContentID != content.ContentID)
            {
                int oldorder = content.Order;
                content.Order = othercontent.Order;
                othercontent.Order = oldorder;
                db.SaveChanges();
            }

            return RedirectToAction("Edit", "Update", new { id = updateid });
        }




        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }
    }
}