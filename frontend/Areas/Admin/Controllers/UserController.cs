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
                        }
                        else
                        {

                            try
                            {

                                using (Image tmp = Image.FromStream(file.InputStream))
                                {
                                    //resize+crop
                                    int width = int.Parse(ConfigurationManager.AppSettings["Image_User_Width"]);
                                    int height = int.Parse(ConfigurationManager.AppSettings["Image_User_Height"]);
                                    string name = file.FileName + ".jpg";
                                    string filepath = string.Format("users/{0}", name);
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
                                    user.Image = address;
                                }


                            }
                            catch (Exception ex)
                            {
                                ModelState.AddModelError(keyname, "Upload error: " + ex.Message);

                            }
                        }

                        ViewBag.CountryID = new SelectList(db.countries, "CountryID", "Code", user.CountryID);
                        ViewBag.Role = new SelectList(db.user_roles, "RoleName", "RoleName");
                        return View(user);
                    }
                }
            }

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
                        }
                        else
                        {

                            try
                            {
                                using (Image tmp = Image.FromStream(file.InputStream))
                                {
                                    //resize+crop
                                    int width = int.Parse(ConfigurationManager.AppSettings["Image_User_Width"]);
                                    int height = int.Parse(ConfigurationManager.AppSettings["Image_User_Height"]);
                                    string name = file.FileName + ".jpg";
                                    string filepath = string.Format("users/{0}", name);
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
                                    user.Image = address;
                                }
                            }
                            catch (Exception ex)
                            {
                                ModelState.AddModelError(keyname, "Upload error: " + ex.Message);
                            }
                        }

                        ViewBag.CountryID = new SelectList(db.countries, "CountryID", "Code", user.CountryID);
                        ViewBag.Role = new SelectList(db.user_roles, "RoleName", "RoleName", user.Role);
                        return View(user);
                    }
                }
            }

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