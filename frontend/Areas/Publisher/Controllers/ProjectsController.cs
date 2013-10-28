using System;
using System.Configuration;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Mvc;
using Amazon.S3;
using Amazon.S3.Model;
using Localactors.entities;

namespace Localactors.webapp.Areas.Publisher.Controllers
{
    [Authorize(Roles = "publisher,admin")]
    [ValidateInput(false)]
    public class ProjectsController : Localactors.webapp.Areas.Publisher.ControllerBase
    {
      
        public ActionResult Index() {
            return RedirectToAction("Index", "Home");


            var projects = db.projects.Include("country").Include("user").Where(x => x.UserID == CurrentUser.UserID);
            return View(projects.ToList());
        }


        

        public ActionResult Create()
        {
            ViewBag.CountryID = new SelectList(db.countries, "CountryID", "Name");

            var model = new project();
            model.UserID = CurrentUser.UserID;
            model.Date = DateTime.Now;
            model.DateStart = DateTime.Now;
            model.DateEnd = DateTime.Now.AddDays(60);
            model.DateUpdate = DateTime.Now;
            model.Target = 1000;
            model.Image = "https://s3-eu-west-1.amazonaws.com/localactors-webapp/projects/placeholder_project.png";

            return View(model);
        } 
        [HttpPost]
        public ActionResult Create(project project)
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
                        else {

                            try {

                                using (Image tmp = Image.FromStream(file.InputStream))
                                {
                                    //resize+crop
                                    int width = int.Parse(ConfigurationManager.AppSettings["Image_Project_Width"]);
                                    int height = int.Parse(ConfigurationManager.AppSettings["Image_Project_Height"]);
                                    string name = getGuid() + ".jpg";
                                    string filepath = string.Format("projects/{0}", name);
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
                                    if (keyname.IndexOf("AgencyLogo") >= 0) {
                                        project.AgencyLogo = address;
                                    }
                                    else {
                                        project.Image = address;
                                    }
                                }

                            }
                            catch (Exception ex) {
                                ModelState.AddModelError(keyname, "Upload error: " + ex.Message);

                            }
                        }

                        ViewBag.CountryID = new SelectList(db.countries, "CountryID", "Name");
                        return View(project);
                    }
                }
            }

            if (ModelState.IsValid) {
                if (project.Image == null)
                    project.Image = "https://s3-eu-west-1.amazonaws.com/localactors-webapp/projects/placeholder_project.png";

                project.UserID = CurrentUser.UserID;
                project.Enabled = false;
                project.DateUpdate = DateTime.Now;
                db.projects.AddObject(project);
                db.SaveChanges();

                SendMailAwsAdmin("New Project", "A new project has been created by a Publisher. You need to check and enable it. Project:" + project.Title);
                TempData["info"] = "Your project has been created and is waiting for approval. Please be patient.";
 
                return RedirectToAction("Edit","Projects",new{id = project.ProjectID,Area="Publisher"});  
            }

            ViewBag.CountryID = new SelectList(db.countries, "CountryID", "Name");
            return View(project);
        }
 
        public ActionResult Edit(int id)
        {
            project project = db.projects
                .Include("tags")
                .Include("project_photo")
                .Include("updates")
                .Include("donations")
                .Include("achievements")
                .Single(p => p.ProjectID == id && p.UserID == CurrentUser.UserID);

            ViewBag.CountryID = new SelectList(db.countries, "CountryID", "Name", project.CountryID);
            ViewBag.Tags = db.tags;
            return View(project);
        }
        [HttpPost]
        public ActionResult Edit(project project)
        {
            var dbproject = db.projects.Single(p => p.ProjectID == project.ProjectID && p.UserID == CurrentUser.UserID);
            if(dbproject == null) {
                return RedirectToAction("Index", "Home", new {Area = "Publisher"});
            }

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
                        else {

                            try {
                                using (Image tmp = Image.FromStream(file.InputStream))
                                {
                                    //resize+crop
                                    int width = int.Parse(ConfigurationManager.AppSettings["Image_Project_Width"]);
                                    int height = int.Parse(ConfigurationManager.AppSettings["Image_Project_Height"]);
                                    string name = getGuid() + ".jpg";
                                    string filepath = string.Format("projects/{0}", name);
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
                                    if (keyname.IndexOf("AgencyLogo") >= 0)
                                    {
                                        project.AgencyLogo = address;
                                    }
                                    else
                                    {
                                        project.Image = address;
                                    }

                                }
                            }
                            catch (Exception ex) {
                                ModelState.AddModelError(keyname, "Upload error: " + ex.Message);
                            }
                        }

                        ViewBag.CountryID = new SelectList(db.countries, "CountryID", "Name", project.CountryID);
                        ViewBag.Tags = db.tags;
                        return View(project);
                    }
                }
            }

            if (ModelState.IsValid) {

                if (project.Image == null)
                    project.Image = "https://s3-eu-west-1.amazonaws.com/localactors-webapp/projects/placeholder_project.png";


                project.UserID = CurrentUser.UserID;
                project.DateUpdate = DateTime.Now;
                db.projects.ApplyCurrentValues(project);
          
                db.SaveChanges();

                SendMailAwsAdmin("Project modified", "A project has been modified by a Publisher. Project:" + project.Title);
                return RedirectToAction("Index", "Home", new { Area = "Publisher" });
            }

            ViewBag.CountryID = new SelectList(db.countries, "CountryID", "Name", project.CountryID);
            ViewBag.Tags = db.tags;
            return View(project);
        }

      

        public ActionResult TagRemove(int id, int projectid)
        {
            project project = db.projects.Single(p => p.ProjectID == projectid && p.UserID == CurrentUser.UserID);
            tag tag = db.tags.Single(x => x.TagID == id);
            project.tags.Remove(tag);
            db.SaveChanges();

            return RedirectToAction("Edit","Projects",new {id = projectid},"tags");
        }
        [HttpPost]
        public ActionResult TagAdd(string tagname, int projectid) {
            var name = tagname.ToLower().Trim();
            name = Regex.Replace(name, @"[^a-zA-Z]", string.Empty);

            if(string.IsNullOrEmpty(name)) {
                return RedirectToAction("Edit", "Projects", new { id = projectid },"tags");
            }

            tag tag = db.tags.FirstOrDefault(x => x.Name.ToLower() == name);

            if(tag == null) {
                tag = new tag(){Name = name};
                db.tags.AddObject(tag);
                //db.SaveChanges();
            }

            project project = db.projects.Single(p => p.ProjectID == projectid && p.UserID == CurrentUser.UserID);
            if(!project.tags.Contains(tag)) {
                project.tags.Add(tag);
                db.SaveChanges();
            }


            return RedirectToAction("Edit","Projects",new {id = projectid},"tags");
        }

        public ActionResult PlanUp(int id) {
            //get the data
            var item = db.project_plan.FirstOrDefault(x => x.PlanID == id);
            var items = db.project_plan.Where(x => x.ProjectID == item.ProjectID).OrderBy("Order");

            project_plan otheritem = null;
            foreach (var updateContent in items)
            {
                if (updateContent.PlanID == id)
                {
                    break;
                }
                otheritem = updateContent;
            }

            if (otheritem != null)
            {
                int oldorder = item.Order;
                item.Order = otheritem.Order;
                otheritem.Order = oldorder;
                db.SaveChanges();
            }

            return RedirectToAction("Edit", "Projects", new { id = item.ProjectID },"plan");
        }
        public ActionResult PlanDown(int id)
        {
            //get the data
            var item = db.project_plan.FirstOrDefault(x => x.PlanID == id);
            var items = db.project_plan.Where(x => x.ProjectID == item.ProjectID).OrderByDescending("Order");

            project_plan otheritem = null;
            foreach (var updateContent in items)
            {
                if (updateContent.PlanID == id)
                {
                    break;
                }
                otheritem = updateContent;
            }

            if (otheritem != null && otheritem.PlanID != otheritem.PlanID)
            {
                int oldorder = item.Order;
                item.Order = otheritem.Order;
                otheritem.Order = oldorder;
                db.SaveChanges();
            }

            return RedirectToAction("Edit", "Projects", new { id = item.ProjectID },"plan");
        }

        public ActionResult CostUp(int id)
        {
            //get the data
            var item = db.project_cost.FirstOrDefault(x => x.CostID == id);
            var items = db.project_cost.Where(x => x.ProjectID == item.ProjectID).OrderBy("Order");

            project_cost otheritem = null;
            foreach (var updateContent in items)
            {
                if (updateContent.CostID == id)
                {
                    break;
                }
                otheritem = updateContent;
            }

            if (otheritem != null)
            {
                int oldorder = item.Order;
                item.Order = otheritem.Order;
                otheritem.Order = oldorder;
                db.SaveChanges();
            }

            return RedirectToAction("Edit", "Projects", new { id = item.ProjectID },"costs");
        }
        public ActionResult CostDown(int id)
        {
            //get the data
            var item = db.project_cost.FirstOrDefault(x => x.CostID == id);
            var items = db.project_cost.Where(x => x.ProjectID == item.ProjectID).OrderByDescending("Order");

            project_cost otheritem = null;
            foreach (var updateContent in items)
            {
                if (updateContent.CostID == id)
                {
                    break;
                }
                otheritem = updateContent;
            }

            if (otheritem != null )
            {
                int oldorder = item.Order;
                item.Order = otheritem.Order;
                otheritem.Order = oldorder;
                db.SaveChanges();
            }

            return RedirectToAction("Edit", "Projects", new { id = item.ProjectID },"costs");
        }


        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }
    }
}