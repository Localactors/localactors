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

        public ViewResult Index(string role = null, int page =1) {

            if (page < 1) page = 1;

            int pagesize = 20;
            int skip = pagesize*(page -1);
            int take = pagesize;


            var users = db.users.Include("country").AsQueryable();
            if (role != null)
                users = users.Where(x => x.Role == role);

            users = users.OrderBy("UserName").Skip(skip).Take(take);
            var list = users.ToList();

            ViewBag.role = role;
            ViewBag.page = page ;
            ViewBag.next = list.Count()>=pagesize;
            ViewBag.prev = page > 1;

            return View(list);
        }


        //
        // GET: /Admin/User/Create

        public ActionResult Create()
        {
            ViewBag.CountryID = new SelectList(db.countries, "CountryID", "Name");
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
                                ModelState.AddModelError(keyname, "Upload error: " + ex.Message + " // " + ex.InnerException.Message);

                            }
                        }

                        ViewBag.CountryID = new SelectList(db.countries, "CountryID", "Name");
                        ViewBag.Role = new SelectList(db.user_roles, "RoleName", "RoleName");
                        return View(user);
                    }
                }
            }else {
                user.Image = "https://s3-eu-west-1.amazonaws.com/localactors-webapp/users/default_user_256.png";
            }

            
            if (ModelState.IsValid) {

                var usr = db.users.FirstOrDefault(x => x.Email.ToLower() == user.Email.ToLower());
                if (usr != null)
                {
                    ModelState.AddModelError("Email", "Indirizzo Email gia registrato");
                    return View(user);
                }

                string key = computeHash(DateTime.Now.ToString("dd/MM/yyHHmmss") + user.Email);
                user newuser = new user
                {
                    UserPassword = computeHash(user.UserPassword.ToUpper()),
                    UserName = user.Email.ToLower(),
                    Name = user.Name,
                    Lastname = user.Lastname,
                    Bio = user.Bio,
                    Image = user.Image,
                    Email = user.Email,
                    CountryID = user.CountryID,
                    DateJoined = DateTime.Now,
                    DateLastLogin = DateTime.Now,
                    Enabled = true,
                    Confirmed = false,
                    Reset = false,
                    Privacy = true,
                    Role = user.Role,
                    Email_Hash = key,
                    Newsletter = user.Newsletter,

                    Contact_Blog = user.Contact_Blog,
                    Contact_Email = user.Contact_Email,
                    Contact_Facebook = user.Contact_Facebook,
                    Contact_Linkedin = user.Contact_Linkedin,
                    Contact_Skype = user.Contact_Skype,
                    Contact_Tel = user.Contact_Tel,
                    Contact_Twitter = user.Contact_Twitter,
                    Contact_Web = user.Contact_Web
                };

                db.users.AddObject(newuser);
                db.SaveChanges();
                return RedirectToAction("Index");  
            }

            ViewBag.CountryID = new SelectList(db.countries, "CountryID", "Name");
            ViewBag.Role = new SelectList(db.user_roles, "RoleName", "RoleName");
            return View(user);
        }
        
        //
        // GET: /Admin/User/Edit/5
 
        public ActionResult Edit(int id)
        {
            user user = db.users.Single(u => u.UserID == id);
            ViewBag.CountryID = new SelectList(db.countries, "CountryID", "Name", user.CountryID);
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
                                ModelState.AddModelError(keyname, "Upload error: " + ex.Message + " // " + ex.InnerException.Message);

                            }
                        }

                        ViewBag.CountryID = new SelectList(db.countries, "CountryID", "Name", user.CountryID);
                        ViewBag.Role = new SelectList(db.user_roles, "RoleName", "RoleName", user.Role);
                        return View(user);
                    }
                }
            }

            if (ModelState.IsValid) {
                //do not change the password!
                //user.UserPassword = computeHash(user.UserPassword.ToUpper());
                user.UserName = user.Email;
                if(string.IsNullOrEmpty( user.Image) || user.Image.Length <=3 )
                    user.Image = "https://s3-eu-west-1.amazonaws.com/localactors-webapp/users/default_user_256.png";

                db.users.Attach(user);
                db.ObjectStateManager.ChangeObjectState(user, EntityState.Modified);
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.CountryID = new SelectList(db.countries, "CountryID", "Name", user.CountryID);
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
            user.Enabled = false;
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        //6620 


        public ActionResult Enable(int id)
        {
            var item = db.users.Single(p => p.UserID == id);
            item.Enabled = true;
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        public ActionResult Disable(int id)
        {
            var item = db.users.Single(p => p.UserID == id);
            item.Enabled = false;
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