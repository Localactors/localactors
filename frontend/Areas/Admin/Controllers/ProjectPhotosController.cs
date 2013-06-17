﻿using System;
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
    public class ProjectPhotosController : ControllerBase
    {
  

        //
        // GET: /Admin/ProjectPhotos/

        public ViewResult Index(int projectid = 0)
        {
            var project_photo = db.project_photo.Include("project");
            ViewBag.Project = db.projects.FirstOrDefault(x => x.ProjectID == projectid);
            return View(project_photo.ToList());
        }

    

        //
        // GET: /Admin/ProjectPhotos/Create

        public ActionResult Create(int projectid = 0, int returntoproject =0)
        {
            ViewBag.ProjectID = new SelectList(db.projects, "ProjectID", "Title");
            ViewBag.returntoproject = returntoproject;
            ViewBag.Project = db.projects.FirstOrDefault(x => x.ProjectID == projectid);

            var model = new project_photo();
            if(projectid>0) {
                model.ProjectID = projectid;
            }

            return View(model);
        } 

        //
        // POST: /Admin/ProjectPhotos/Create

        [HttpPost]
        public ActionResult Create(project_photo project_photo, int projectid = 0, int returntoproject = 0)
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
                        if (ext == ".png" || ext == ".jpg" || ext == ".jpeg" || ext == ".gif") {
                            try {
                                using (Image tmp = Image.FromStream(file.InputStream))
                                {
                                    //resize+crop
                                    int width = int.Parse(ConfigurationManager.AppSettings["Image_Gallery_Width"]);
                                    int height = int.Parse(ConfigurationManager.AppSettings["Image_Gallery_Height"]);
                                    string name = file.FileName + ".jpg";
                                    string filepath = string.Format("projects/{0}/photos/{1}", project_photo.ProjectID, name);
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
                                    project_photo.Url = address;

                                }
                            }
                            catch (Exception ex) {
                                ModelState.AddModelError(keyname, "Upload error: " + ex.Message);
                            }
                        }else {
                            ModelState.AddModelError(keyname, "Invalid file type");
                        }
                    }
                }
            }

            if (ModelState.IsValid)
            {
                db.project_photo.AddObject(project_photo);
                db.SaveChanges();

                if(returntoproject>0) {
                    return RedirectToAction("Edit", "Projects", new { id = project_photo.ProjectID});
                }

                return RedirectToAction("Index");  
            }

            ViewBag.ProjectID = new SelectList(db.projects, "ProjectID", "Title", project_photo.ProjectID);
            ViewBag.returntoproject = returntoproject;
            ViewBag.Project = db.projects.FirstOrDefault(x => x.ProjectID == project_photo.ProjectID);
            return View(project_photo);
        }
        
        //
        // GET: /Admin/ProjectPhotos/Edit/5

        public ActionResult Edit(int id, int projectid = 0, int returntoproject = 0)
        {
            project_photo project_photo = db.project_photo.Single(p => p.PhotoID == id);
            ViewBag.ProjectID = new SelectList(db.projects, "ProjectID", "Title", project_photo.ProjectID);
            ViewBag.returntoproject = returntoproject;
            ViewBag.Project = db.projects.FirstOrDefault(x => x.ProjectID == project_photo.ProjectID);
            return View(project_photo);
        }

        //
        // POST: /Admin/ProjectPhotos/Edit/5

        [HttpPost]
        public ActionResult Edit(project_photo project_photo, int projectid = 0, int returntoproject = 0)
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
                            ViewBag.ProjectID = new SelectList(db.projects, "ProjectID", "Title", project_photo.ProjectID);
                            ViewBag.returntoproject = returntoproject;
                            ViewBag.Project = db.projects.FirstOrDefault(x => x.ProjectID == project_photo.ProjectID);
                            return View(project_photo);
                        }

                        try
                        {
                            using (Image tmp = Image.FromStream(file.InputStream))
                            {
                                //resize+crop
                                int width = int.Parse(ConfigurationManager.AppSettings["Image_Gallery_Width"]);
                                int height = int.Parse(ConfigurationManager.AppSettings["Image_Gallery_Height"]);
                                string name = file.FileName + ".jpg";
                                string filepath = string.Format("projects/{0}/photos/{1}", project_photo.ProjectID, name);
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
                                project_photo.Url = address;

                            }
                        }
                        catch (Exception ex)
                        {
                            ModelState.AddModelError(keyname, "Upload error: " + ex.Message);
                            ViewBag.ProjectID = new SelectList(db.projects, "ProjectID", "Title", project_photo.ProjectID);
                            ViewBag.returntoproject = returntoproject;
                            ViewBag.Project = db.projects.FirstOrDefault(x => x.ProjectID == project_photo.ProjectID);
                            return View(project_photo);
                        }
                    }
                }
            }


            if (ModelState.IsValid)
            {
                db.project_photo.Attach(project_photo);
                db.ObjectStateManager.ChangeObjectState(project_photo, EntityState.Modified);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.ProjectID = new SelectList(db.projects, "ProjectID", "Title", project_photo.ProjectID);
            ViewBag.returntoproject = returntoproject;
            ViewBag.Project = db.projects.FirstOrDefault(x => x.ProjectID == project_photo.ProjectID);
            return View(project_photo);
        }

        //
        // GET: /Admin/ProjectPhotos/Delete/5

        public ActionResult Delete(int id, int projectid = 0, int returntoproject = 0)
        {
            project_photo project_photo = db.project_photo.Single(p => p.PhotoID == id);
            ViewBag.returntoproject = returntoproject;
            ViewBag.Project = db.projects.FirstOrDefault(x => x.ProjectID == project_photo.ProjectID);
            return View(project_photo);
        }

        //
        // POST: /Admin/ProjectPhotos/Delete/5

        [HttpPost, ActionName("Delete")]
        public ActionResult DeleteConfirmed(int id, int projectid = 0, int returntoproject = 0)
        {            
            project_photo project_photo = db.project_photo.Single(p => p.PhotoID == id);
            db.project_photo.DeleteObject(project_photo);
            db.SaveChanges();

            if (returntoproject > 0)
            {
                return RedirectToAction("Edit", "Projects", new { id = project_photo.ProjectID });
            }

            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }
    }
}