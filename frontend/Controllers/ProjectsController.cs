﻿using System;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Amazon.S3;
using Amazon.S3.Model;
using Localactors.entities;
using System.Drawing;

namespace Localactors.webapp.Controllers
{ 
    public class ProjectsController : ControllerBase
    {

        [OutputCache(VaryByParam = "*", Duration = 60)]
        public ViewResult Index(string tag = null)
        {
            var projects = db.projects
                .Include("country")
                .Include("user")
                .Include("tags").Where(x => x.Enabled);

            if(tag!=null) {
                projects = projects.Where(x => x.tags.Any(y => y.Name == tag));
            }

            projects = projects.OrderBy("DateUpdate");

            return View(projects.ToList());
        }

   
        [OutputCache(VaryByParam = "*", Duration = 60)]
        public ViewResult Details(int id)
        {
            project project = db.projects
                .Include("country")
                .Include("user")
                .Include("project_guestbook")
                .Include("project_photo")
                .Include("tags")
                .Include("updates")
                .Include("achievements")
                .Single(p => p.ProjectID == id);

            ViewBag.UserID = CurrentUser != null ? CurrentUser.UserID : 0;
            return View(project);
        }

        [OutputCache(VaryByParam = "*", Duration = 60)]
        public ViewResult Donate(int id)
        {
            project project = db.projects
                .Include("country")
                .Include("user")
                .Single(p => p.ProjectID == id);

            ViewBag.UserID = CurrentUser != null ? CurrentUser.UserID : 0;
            return View(project);
        }

        public ViewResult ThankYou(int id) {
            var model = db.projects.FirstOrDefault(x => x.ProjectID == id);
            return View(model);
        }

        [OutputCache(VaryByParam = "*",Duration = 60)]
        public ViewResult Updates(int id)
        {
            project project = db.projects
                //.Include("country")
                .Include("user")
                //.Include("project_guestbook")
                //.Include("project_photo")
                .Include("tags")
                .Include("updates")
                //.Include("achievements")
                .FirstOrDefault(p => p.ProjectID == id );

            ViewBag.UserID = CurrentUser!=null ? CurrentUser.UserID : 0;
            return View(project);
        }

        public class AskModel {
            public AskQuestion Question { get; set; }
            public project Project { get; set; }
        }
        [OutputCache(VaryByParam = "*", Duration = 60)]
        public ActionResult Ask(int id)
        {
            project project = db.projects
                .Include("country")
                .Include("user")
                .Include("project_guestbook")
                .Include("project_photo")
                .Include("tags")
                .Include("updates")
                .Include("achievements")
                .Single(p => p.ProjectID == id);
            ViewBag.UserID = CurrentUser != null ? CurrentUser.UserID : 0;
            return View(new AskModel() { Project = project, Question = new AskQuestion(){ProjectID = id,ProjectName = project.Title,UserName = User.Identity.Name} });
        }
        [HttpPost]
        public ActionResult Ask(AskQuestion question)
        {
            project project = db.projects
                .Include("country")
                .Include("user")
                .Include("project_guestbook")
                .Include("project_photo")
                .Include("tags")
                .Include("updates")
                .Include("achievements")
                .Single(p => p.ProjectID == question.ProjectID);

            AskModel ask = new AskModel() { Project = project, Question = question };

            if(ModelState.IsValid) {
                //send email
                string body = string.Format("From: {0}\r\nName (if loggedin): {1}\r\nProject: {2}\r\nProjectID: {3}\r\n\r\nQuestion: {4}",question.Email, question.UserName,question.ProjectName,question.ProjectID,question.Question );
                SendMailAws(ConfigurationManager.AppSettings["Email_Info"], "Question about project: " + question.ProjectName, body);
                return RedirectToAction("Details", "Projects", new {id = question.ProjectID});
            }

            return View(ask);
        }

        [Authorize(Roles = "supporter,publisher,admin")]
        public ActionResult Follow(int id) {
            project project = db.projects.Single(p => p.ProjectID == id);

            if (project != null && !CurrentUser.followedProjects.Any(x => x.ProjectID == id)) {
                CurrentUser.followedProjects.Add(project);
                db.SaveChanges();
            }

            return Request.UrlReferrer!=null ? Redirect(Request.UrlReferrer.AbsolutePath):Redirect("/");

        }


        [OutputCache(VaryByParam = "*", Duration = 60)]
        public ViewResult Guestbook(int id)
        {
            project project = db.projects
                .Include("country")
                .Include("user")
                .Include("project_guestbook")
                .Include("tags")
                .Single(p => p.ProjectID == id);

            ViewBag.UserID = CurrentUser != null ? CurrentUser.UserID : 0;
            return View(project);
        }
        [HttpPost]
        [Authorize(Roles="supporter,publisher,admin")]
        public ActionResult GuestbookCreate(project_guestbook model)
        {
            project project = db.projects
                .Single(p => p.ProjectID == model.ProjectID);

            model.UserID = CurrentUser.UserID;
            model.Date = DateTime.Now;

            ModelState.Remove("UserID");
            ModelState.Add("UserID", new ModelState());
            ModelState.SetModelValue("UserID", new ValueProviderResult(CurrentUser.UserID, CurrentUser.UserID.ToString(), null));

            ModelState.Remove("Date");
            ModelState.Add("Date", new ModelState());
            ModelState.SetModelValue("Date", new ValueProviderResult(DateTime.Now, DateTime.Now.ToString(), null));

            ViewBag.guestbook_text = model.Text;
            ViewBag.guestbook_image = model.Picture;

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
                                using (Image tmp = Image.FromStream(file.InputStream)) {
                                    //resize+crop
                                    int width = int.Parse(ConfigurationManager.AppSettings["Image_Guestbook_Width"]);
                                    int height = int.Parse(ConfigurationManager.AppSettings["Image_Guestbook_Height"]);
                                    string name = getTimestamp() + ".jpg";
                                    string filepath = string.Format("projects/{0}/guestbook/{1}", model.ProjectID, file.FileName);
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
                                    model.Picture = address;

                      
                                }
                            }
                            catch (Exception ex)
                            {
                                ModelState.AddModelError(keyname, "Upload error: " + ex.Message);

                            }
                        }

                    }
                }
            }

            if (ModelState.IsValid )
            {
                project.project_guestbook.Add(model);
                db.SaveChanges();
            }

            //redirect & reload
            if (Request.UrlReferrer != null)
            {
                HttpResponse.RemoveOutputCacheItem(Request.UrlReferrer.AbsolutePath);
                return Redirect(ReferrerUrlTimestamped());
            }
            return Redirect("/");
        }
        [HttpPost]
        [Authorize()]
        public ActionResult GuestbookDelete(int GuestpostID, int ProjectID)
        {
            var post = db.project_guestbook.FirstOrDefault(x => x.GuestpostID == GuestpostID);
            if (post!=null && (User.IsInRole("admin") || post.UserID == CurrentUser.UserID || post.project.UserID == CurrentUser.UserID)) {
                db.project_guestbook.DeleteObject(post);
                db.SaveChanges();
            }

            if (Request.UrlReferrer != null)
            {
                HttpResponse.RemoveOutputCacheItem(Request.UrlReferrer.AbsolutePath);
                return Redirect(ReferrerUrlTimestamped());
            }
            return Redirect("/");
        }

        [HttpPost]
        [Authorize(Roles = "supporter,publisher,admin")]
        public ActionResult CommentCreate(update_comment model) {
            update update = db.updates.FirstOrDefault(x => x.UpdateID == model.UpdateID);

            model.UserID = CurrentUser.UserID;
            model.Date = DateTime.Now;
            model.Picture = null;

            ModelState.Remove("UserID");
            ModelState.Add("UserID", new ModelState());
            ModelState.SetModelValue("UserID", new ValueProviderResult(CurrentUser.UserID, CurrentUser.UserID.ToString(), null));

            ModelState.Remove("Date");
            ModelState.Add("Date", new ModelState());
            ModelState.SetModelValue("Date", new ValueProviderResult(DateTime.Now, DateTime.Now.ToString(), null));


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
                                    int width = int.Parse(ConfigurationManager.AppSettings["Image_Comment_Width"]);
                                    int height = int.Parse(ConfigurationManager.AppSettings["Image_Comment_Height"]);
                                    string name = getTimestamp() + ".jpg";
                                    string filepath = string.Format("projects/{0}/update/{1}/comments/{2}", update.ProjectID, update.UpdateID, file.FileName);
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
                                    model.Picture = address;
                                }
                            }
                            catch (Exception ex)
                            {
                                ModelState.AddModelError(keyname, "Upload error: " + ex.Message);

                            }
                        }

                    }
                }
            }

            if (ModelState.IsValid)
            {
                update.update_comment.Add(model);
                db.SaveChanges();
            }

            if (Request.UrlReferrer != null) {
                HttpResponse.RemoveOutputCacheItem(Request.UrlReferrer.AbsolutePath);
                return Redirect(ReferrerUrlTimestamped() + "#comments-" + model.update.ProjectID);
            }
            return Redirect("/");
        }

        [HttpPost]
        [Authorize()]
        public ActionResult CommentDelete(int CommentID, int ProjectID) {
            var comment = db.update_comment.FirstOrDefault(x => x.CommentID == CommentID);

            if (comment != null && (User.IsInRole("admin") || 
                comment.UserID == CurrentUser.UserID || 
                comment.update.UserID == CurrentUser.UserID ))
            {
                db.update_comment.DeleteObject(comment);
                db.SaveChanges();
            }


          

            if (Request.UrlReferrer != null)
            {
                HttpResponse.RemoveOutputCacheItem(Request.UrlReferrer.AbsolutePath);
                return Redirect(ReferrerUrlTimestamped() + "#comments-" + ProjectID);
            }
            return Redirect("/");
        }

        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }
    }
}