using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.Entity;
using System.Drawing.Imaging;
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

        public ViewResult Index(string tag = null)
        {
            var projects = db.projects
                .Include("country")
                .Include("user")
                .Include("tags").AsQueryable();

            if(tag!=null) {
                //projects = db.tags.Where(x => x.Name.ToLower() == tag.ToLower()).SelectMany(x => x.projects).Distinct().ToList();
                projects = projects.Where(x => x.tags.Any(y => y.Name == tag));
            }

            return View(projects.ToList());
        }

        //
        // GET: /Projects/Details/5

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
            return View(project);
        }

        public ViewResult Updates(int id)
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
            return View(project);
        }


        [HttpPost]
        [Authorize(Roles="supporter,publisher,admin")]
        public ActionResult GuestbookCreate(project_guestbook model)
        {
            project project = db.projects
                .Include("country")
                .Include("user")
                .Include("project_guestbook")
                .Include("project_photo")
                .Include("tags")
                .Include("updates")
                .Include("achievements")
                .Single(p => p.ProjectID == model.ProjectID);

            model.UserID = CurrentUser.UserID;
            model.Date = DateTime.Now;

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
                                using (Image tmp = Image.FromStream(file.InputStream)) {
                                    //resize+crop
                                    int width = int.Parse(ConfigurationManager.AppSettings["Image_Guestbook_Width"]);
                                    int height = int.Parse(ConfigurationManager.AppSettings["Image_Guestbook_Height"]);
                                    string name = file.FileName + ".jpg";
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

            if (ModelState.IsValid)
            {
                project.project_guestbook.Add(model);
                db.SaveChanges();
            }

            return RedirectToAction("Details", new {id = model.ProjectID});
            return View("Details", project);
        }
        

        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }
    }
}