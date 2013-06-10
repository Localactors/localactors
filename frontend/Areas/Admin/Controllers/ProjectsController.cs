﻿using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.Entity;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Mvc;
using Amazon.S3;
using Amazon.S3.Model;
using Localactors.entities;

namespace Localactors.webapp.Areas.Admin.Controllers
{
    [Authorize(Roles = "admin")]
    public class ProjectsController : ControllerBase
    {
      
        public ViewResult Index()
        {
            var projects = db.projects.Include("country").Include("user");
            return View(projects.ToList());
        }

        public ViewResult Details(int id)
        {
            project project = db.projects.Single(p => p.ProjectID == id);
            return View(project);
        }

        public ActionResult Create()
        {
            ViewBag.CountryID = new SelectList(db.countries, "CountryID", "Code");
            ViewBag.UserID = new SelectList(db.users.Where(x => x.Role == "admin" || x.Role == "publisher"), "UserID", "UserName");
            var model = new project();
            model.Date = DateTime.Now;
            model.DateStart = DateTime.Now;
            model.DateEnd = DateTime.Now.AddDays(60);
            model.Target = 1000;
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
                                    string name = file.FileName + ".jpg";
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
                                    project.Image = address;
   
                                }

                            }
                            catch (Exception ex) {
                                ModelState.AddModelError(keyname, "Upload error: " + ex.Message);

                            }
                        }

                        ViewBag.CountryID = new SelectList(db.countries, "CountryID", "Code", project.CountryID);
                        ViewBag.UserID = new SelectList(db.users.Where(x => x.Role == "admin" || x.Role == "publisher"), "UserID", "UserName", project.UserID);
                        return View(project);
                    }
                }
            }

            if (ModelState.IsValid)
            {
                db.projects.AddObject(project);
                db.SaveChanges();
                //return RedirectToAction("Index");  
                return RedirectToAction("Create","Update",new{projectid = project.ProjectID});  
            }

            ViewBag.CountryID = new SelectList(db.countries, "CountryID", "Code", project.CountryID);
            ViewBag.UserID = new SelectList(db.users.Where(x => x.Role == "admin" || x.Role == "publisher"), "UserID", "UserName", project.UserID);
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
                .Single(p => p.ProjectID == id);

            ViewBag.Supporters = new SelectList(db.users.Where(x => x.Role == "admin" || x.Role == "supporter"), "UserID", "UserName");
            ViewBag.CountryID = new SelectList(db.countries, "CountryID", "Code", project.CountryID);
            ViewBag.UserID = new SelectList(db.users.Where(x => x.Role == "admin" || x.Role == "publisher"), "UserID", "UserName", project.UserID);
            ViewBag.Tags = db.tags;
            return View(project);
        }
        [HttpPost]
        public ActionResult Edit(project project)
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
                                    string name = file.FileName + ".jpg";
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
                                    project.Image = address;

                                }
                            }
                            catch (Exception ex) {
                                ModelState.AddModelError(keyname, "Upload error: " + ex.Message);
                            }
                        }

                        ViewBag.Supporters = new SelectList(db.users.Where(x => x.Role == "admin" || x.Role == "supporter"), "UserID", "UserName");
                        ViewBag.CountryID = new SelectList(db.countries, "CountryID", "Code", project.CountryID);
                        ViewBag.UserID = new SelectList(db.users.Where(x => x.Role == "admin" || x.Role == "publisher"), "UserID", "UserName", project.UserID);
                        ViewBag.Tags = db.tags;
                        return View(project);
                    }
                }
            }

            if (ModelState.IsValid)
            {
                db.projects.Attach(project);
                db.ObjectStateManager.ChangeObjectState(project, EntityState.Modified);
                db.SaveChanges();
            }

            ViewBag.Supporters = new SelectList(db.users.Where(x => x.Role == "admin" || x.Role == "supporter"), "UserID", "UserName");
            ViewBag.CountryID = new SelectList(db.countries, "CountryID", "Code", project.CountryID);
            ViewBag.UserID = new SelectList(db.users.Where(x => x.Role == "admin" || x.Role == "publisher"), "UserID", "UserName", project.UserID);
            ViewBag.Tags = db.tags;
            return View(project);
        }

        //
        // GET: /Admin/Projects/Delete/5
 
        public ActionResult Delete(int id)
        {
            project project = db.projects.Single(p => p.ProjectID == id);
            return View(project);
        }

        //
        // POST: /Admin/Projects/Delete/5

        [HttpPost, ActionName("Delete")]
        public ActionResult DeleteConfirmed(int id)
        {
            project project = db.projects.Single(p => p.ProjectID == id);

            //{
            //    var list = project.updates.SelectMany(x => x.update_content);
            //    foreach (var item in list)
            //        db.update_content.DeleteObject(item);
            //}

            //var list = project.updates.SelectMany(x => x.update_content);
            //foreach (var item in project.updates.SelectMany(x => x.update_comment))
            //    db.update_comment.DeleteObject(item);

            //foreach (var item in project.updates)
            //    db.updates.DeleteObject(item);

            //foreach (var item in project.tags)
            //    project.tags.Remove(item);

            //foreach (var item in project.project_photo)
            //    project.project_photo.Remove(item);

            //foreach (var item in project.donations)
            //    project.donations.Remove(item);

            //foreach (var item in project.achievements)
            //    project.achievements.Remove(item);

            //foreach (var item in project.followers)
            //    project.followers.Remove(item);

            //foreach (var item in project.project_guestbook)
            //    project.project_guestbook.Remove(item);

            db.projects.DeleteObject(project);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        //////////////
        //TAGS////////
        //////////////
        public ActionResult TagRemove(int id, int projectid)
        {
            project project = db.projects.Single(p => p.ProjectID == projectid);
            tag tag = db.tags.Single(x => x.TagID == id);
            project.tags.Remove(tag);
            db.SaveChanges();

            return RedirectToAction("Edit","Projects",new {id = projectid});
        }

        [HttpPost]
        public ActionResult TagAdd(string tagname, int projectid) {
            var name = tagname.ToLower().Trim();
            name = Regex.Replace(name, @"[^a-zA-Z]", string.Empty);

            if(string.IsNullOrEmpty(name)) {
                return RedirectToAction("Edit", "Projects", new { id = projectid });
            }

            tag tag = db.tags.FirstOrDefault(x => x.Name.ToLower() == name);

            if(tag == null) {
                tag = new tag(){Name = name};
                db.tags.AddObject(tag);
                //db.SaveChanges();
            }

            project project = db.projects.Single(p => p.ProjectID == projectid);
            if(!project.tags.Contains(tag)) {
                project.tags.Add(tag);
                db.SaveChanges();
            }


            return RedirectToAction("Edit","Projects",new {id = projectid});
        }

        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }
    }
}