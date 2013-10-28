using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.Entity;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.Caching;
using System.Web;
using System.Web.Mvc;
using Amazon.S3;
using Amazon.S3.Model;
using Localactors.entities;
using WSC_webapp.Models;

namespace Localactors.webapp.Areas.Publisher.Controllers
{
    [Authorize]
    public class HomeController : Localactors.webapp.Areas.Publisher.ControllerBase
    {
        public ActionResult Index() {

            var user = db.users.FirstOrDefault(x => x.UserName == User.Identity.Name);

            if(string.IsNullOrEmpty(user.Name) || string.IsNullOrEmpty(user.Lastname) || string.IsNullOrEmpty(user.Image)) {
                TempData["warning"] = "You need to update your profile to better sponsor your project! <a href='"+ Url.Action("Settings","Home") +"'>click here</a>";
            }

            if(user == null) {
                return Redirect("/");
            }

            if(user.Role!="publisher") {
                user.Role = "publisher";
                user.EnablePublisher = false;
                db.SaveChanges();
            }

            var projects = db.projects.Where(x=>x.UserID == user.UserID).Include("country").Include("user");
            return View(projects.ToList());
        }





        //User Settings
        public ActionResult Settings()
        {
            user user = db.users.FirstOrDefault(x => x.UserName == User.Identity.Name);
            if (user == null)
                return Redirect("/");

            ViewBag.CountryID = new SelectList(db.countries, "CountryID", "Name", user.CountryID);
            return View(user);
        }
        [HttpPost]
        public ActionResult Settings(user model)
        {
            user user = db.users.FirstOrDefault(x => x.UserName == User.Identity.Name);
            if (user == null)
                return RedirectToAction("Index", "Home");

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
                                    string name = getGuid() + ".jpg";
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

                        ViewBag.CountryID = new SelectList(db.countries, "CountryID", "Name", user.CountryID);
                        return View(user);
                    }
                }
            }

            if (ModelState.IsValid)
            {
                user.Name = model.Name;
                user.Lastname = model.Lastname;
                user.Bio = model.Bio;
                user.Image = model.Image;
                user.CountryID = model.CountryID;

                user.Contact_Blog = model.Contact_Blog;
                user.Contact_Email = model.Contact_Email;
                user.Contact_Facebook = model.Contact_Facebook;
                user.Contact_Linkedin = model.Contact_Linkedin;
                user.Contact_Skype = model.Contact_Skype;
                user.Contact_Tel = model.Contact_Tel;
                user.Contact_Twitter = model.Contact_Twitter;
                user.Contact_Web = model.Contact_Web;

                db.ObjectStateManager.ChangeObjectState(user, EntityState.Modified);
                db.SaveChanges();

                //clear
                OutputCacheAttribute.ChildActionCache = new MemoryCache("newcache");
            }

            ViewBag.CountryID = new SelectList(db.countries, "CountryID", "Name", user.CountryID);
            return View(user);
        }

    
    }
}
