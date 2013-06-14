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

namespace Localactors.webapp.Controllers
{ 
    
    public class ProfileModel {
        public user user { get; set; }
        public List<project> projects { get; set; }
        public List<donation> donations { get; set; }
        public List<update> updates { get; set; }
    }

    public class LocalactorsModel
    {
        public List<user> publishers { get; set; }
        public List<user> supporters { get; set; }
    }

    public class ProfileBarModel
    {
        public user user { get; set; }
        public List<update> updates { get; set; }
        public List<donation> donations { get; set; }
        public List<project> projects { get; set; }
    }

    public class ProfileController : ControllerBase
    {
        [ChildActionOnly]
        [OutputCache(Duration = 60, VaryByParam = "*")]
        public PartialViewResult ProfileBar(string username) {
            var user = db.users.FirstOrDefault(x => x.UserName == username);

            ProfileBarModel model = new ProfileBarModel();
            model.user = user;
            if(user!=null) {
                model.donations = user.donations.ToList();
                model.projects = user.donations.Select(x=>x.project).Distinct().ToList();
                model.updates = user.followedProjects.SelectMany(x => x.updates).OrderByDescending(x => x.UpdateID).Skip(0).Take(5).ToList();
            }

            return PartialView("_ProfileBar", model);
        }

        [OutputCache(VaryByParam = "*", Duration = 60)]
        public ViewResult Index()
        {
            LocalactorsModel model = new LocalactorsModel();
            model.publishers = db.users.Include("projects").Where(x => x.Role == "publisher").ToList();
            model.supporters = db.users.Where(x => x.Role == "supporter").ToList();
            return View(model);
        }

        [OutputCache(VaryByParam = "*", Duration = 60)]
        public ViewResult Details(int id)
        {
            user user = db.users.Single(u => u.UserID == id);
            return View(user);
        }

        [Authorize]
        public ViewResult Feed(int page=1) {
            var realpage = page - 1;
            int pagesize = int.Parse(ConfigurationManager.AppSettings["Pagesize_UserUpdates"]);
            int skip = realpage * pagesize;
            int take = pagesize;

            user user = db.users.Single(u => u.UserName == User.Identity.Name);

            ProfileModel model = new ProfileModel();
            model.user = db.users.Single(u => u.UserName == User.Identity.Name);
            //model.projects = model.user.donations.Select(x => x.project).Distinct().ToList();
            //model.donations = model.user.donations.ToList();
            model.updates = model.user.followedProjects.SelectMany(x => x.updates).OrderByDescending(x => x.UpdateID).Skip(skip).Take(take).ToList();

            ViewBag.page = page;
            ViewBag.hasnext = model.updates.Count >= pagesize;

            //add some updates if there are less than pagesize, but leave the pager disabled :)
            if (model.updates.Count < pagesize) {
                int many = pagesize - model.updates.Count;
                if(many >0) {
                    var add = db.updates.Except(model.updates).OrderByDescending("UpdateID").Take(many);
                    model.updates.AddRange(add);
                }
            }

            return View(model);
        }

        [Authorize]
        public ActionResult Settings()
        {
            user user = db.users.FirstOrDefault(x => x.UserName == User.Identity.Name);
            ViewBag.CountryID = new SelectList(db.countries, "CountryID", "Name", user.CountryID);
            return View(user);
        }
        [Authorize]
        [HttpPost]
        public ActionResult Settings(user model) {
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

                        ViewBag.CountryID = new SelectList(db.countries, "CountryID", "Name", user.CountryID);
                        return View(user);
                    }
                }
            }

            if (ModelState.IsValid) {
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
            }

            ViewBag.CountryID = new SelectList(db.countries, "CountryID", "Name", user.CountryID);
            return View(user);
        }


        

        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }
    }
}