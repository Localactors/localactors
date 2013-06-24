using System;
using System.Configuration;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using Localactors.entities;
using WSC_webapp.Models;

namespace Localactors.webapp.Controllers
{

    public class AccountController : ControllerBase
    {

        public ActionResult Index()
        {
            Response.Cache.SetCacheability(HttpCacheability.NoCache);

            if (!Request.IsAuthenticated)
                return RedirectToAction("Index", "Home");

            user user = db.users.FirstOrDefault(x => x.UserName == User.Identity.Name);
            if (user == null)
                return RedirectToAction("Index", "Home");

            return View(user);
        }

        public ActionResult Details(string username)
        {
            //if (!Request.IsAuthenticated)
            //    return RedirectToAction("Index", "Home");
            Response.Cache.SetCacheability(HttpCacheability.NoCache);

            user user = db.users.FirstOrDefault(x => x.UserName == username && x.Enabled);
            if (user == null)
                return RedirectToAction("Index", "Home");

            return View(user);
        }

        //LOGIN
        [HttpGet]
        public ActionResult Login(string ReturnUrl)
        {
            Response.Cache.SetCacheability(HttpCacheability.NoCache);

            LoginModel model = new LoginModel();
            model.ReturnUrl = ReturnUrl;

            return View(model);
        }
        [HttpPost]
        public ActionResult Login(LoginModel model)
        {

            Response.Cache.SetCacheability(HttpCacheability.NoCache);

            if (!ModelState.IsValid)
                return View(model);

            user geo = db.users.FirstOrDefault( x=> ( x.UserName.ToLower() == model.Username.ToLower() || x.Email.ToLower() == model.Username.ToLower()));

            if (geo == null)
            {
                ModelState.AddModelError("Username", "wrong user or password");
                return View(model);
            }

            if(geo.Confirmed == false ){
                ModelState.AddModelError("Username","User not yet confirmed. Check your inbox for the confirmation email.");
                return View(model);
            }

            if (geo.Enabled == false)
            {
                ModelState.AddModelError("Username", "user not enabled");
                return View(model);
            }

            string sha = computeHash(model.Password.ToUpper());
            if (geo.UserPassword == sha || (geo.UserPassword.Length < 10 && geo.UserPassword.ToUpper() == model.Password.ToUpper()))
            {
                geo.Reset = false;
                geo.Email_Hash = null;
                geo.DateLastLogin = DateTime.Now;
                db.SaveChanges();

                //TempData["success"] = "Accesso effettuato, benvenuto " + geo.UserName;
                //if (geo.ShouldChangePassword) {
                //    return RedirectToAction("ShouldChangePassword", "Account",new{username = model.Username});
                //}

                FormsAuthentication.SetAuthCookie(model.Username.ToLower(), model.RememberMe);
                
                //data cookies
                PushCookies(geo);

                if (!string.IsNullOrEmpty(model.ReturnUrl))
                    return Redirect(model.ReturnUrl);
                else
                    return RedirectToAction("Index", "Home");
            }



            //fallback out!
            TempData["error"] = "Errore di accesso";
            ModelState.AddModelError("Username", "wrong username or password");
            return View(model);
        }
        [HttpGet]
        public ActionResult Logout()
        {
            Response.Cache.SetCacheability(HttpCacheability.NoCache);

            //remove cookies
            PushCookies(null);

            FormsAuthentication.SignOut();
            return RedirectToAction("Index", "Home");
        }

        //SUBSCRIBE
        [HttpGet]
        public ActionResult Subscribe()
        {

            return View();
        }
        [HttpPost]
        public ActionResult Subscribe(SubscribeModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            if (model.Password != model.Confirm)
            {
                ModelState.AddModelError("Confirm", "Passwords do not match");
                return View(model);
            }

            if (!model.Terms.Equals("I Agree", StringComparison.InvariantCultureIgnoreCase))
            {
                ModelState.AddModelError("Terms", "You need to read and agree to the terms and conditions.");
                return View(model);
            }

            //Regex regex = new Regex("^[a-zA-Z0-9_\\.]*$", RegexOptions.IgnoreCase);
            //Match m = regex.Match(model.Username);
            //if (!m.Success || model.Username.Length < 3 || model.Username.Length > 15)
            //{
            //    ModelState.AddModelError("Username", "Nome Utente non consentito");
            //    return View(model);
            //}

            //user geo = db.users.FirstOrDefault(x => x.UserName == model.Username);
            //if (geo != null)
            //{
            //    ModelState.AddModelError("Username", "Nome Utente gia registrato");
            //    return View(model);
            //}
            var geo = db.users.FirstOrDefault(x => x.Email.ToLower() == model.Email.ToLower());
            if (geo != null)
            {
                ModelState.AddModelError("Email", "Email address already registered");
                return View(model);
            }


            ////check captcha
            //string ip = Request.UserHostAddress;
            //string captchaurl =     ConfigurationManager.AppSettings["recaptcha_url"];
            //string captchakey =     ConfigurationManager.AppSettings["recaptcha_key"];
            //string captchasecret =  ConfigurationManager.AppSettings["recaptcha_secret"];
            //ReCaptcha.api.Configuration config = new ReCaptcha.api.Configuration(captchaurl, captchakey, captchasecret, null);
            //ReCaptchaApiWrapper api = ReCaptchaApiWrapper.GetInstance(config);
            //ReCaptchaResponse response = api.VerifyCaptcha(ip,model.recaptcha_challenge_field,model.recaptcha_response_field);

            //if (!response.success) {
            //    ModelState.AddModelError("recaptcha_challenge_field", "La chiave inserita e l'immagine non corrispondono.");
            //    return View(model);
            //}

            string key = computeHash(DateTime.Now.ToString("dd/MM/yyHHmmss") + model.Email);
            user newuser = new user
                                  {
                                      UserPassword = computeHash(model.Password.ToUpper()),
                                      UserName = model.Email.ToLower(),
                                      Name = "",
                                      Lastname = "",
                                      Email = model.Email,
                                      DateJoined = DateTime.Now,
                                      DateLastLogin = DateTime.Now,
                                      Enabled = true,
                                      Confirmed = false,
                                      Reset = false,
                                      Role = "supporter",
                                      Email_Hash = key,
                                      Newsletter = model.Newsletter,
                                      Image = "https://s3-eu-west-1.amazonaws.com/localactors-webapp/users/default_user_256.png"
                                  };

            if (model.Privacy.Equals("I Agree", StringComparison.InvariantCultureIgnoreCase))
            {
                newuser.Privacy = true;
            }

            db.users.AddObject(newuser);
            db.SaveChanges();

            //activation
            string url = Url.Action("SubscribeConfirm", "Account", new { key }, "http");
            string body = string.Format("Hi, \r\n\r\nTo activate your account you should click this link :\r\n\r\n{0}\r\n\r\nOr manually insert the code in the confirmation box \r\ncode:{1}", url, key);
            SendMailAws(newuser.Email, "LocalActors: Email Confirmation", body);

            //admins
            SendMailAwsAdmin("LocalActors New User:" + newuser.UserName,"");

            FormsAuthentication.SetAuthCookie(model.Email, true);
            return RedirectToAction("Settings", "Profile");

        }
        [HttpGet]
        public ActionResult SubscribeConfirm(string key = null)
        {

            if (key == null)
                key = "";

            ViewBag.key = key;

            if (!string.IsNullOrEmpty(key))
            {
                user geo = db.users.FirstOrDefault(x => x.Email_Hash == key && x.Confirmed == false);
                if (geo == null)
                {
                    ModelState.AddModelError(string.Empty, "Wrong code or account already active");
                    return View();
                }

                geo.Role = "supporter";
                geo.Enabled = true;
                geo.Confirmed = true;
                geo.Reset = false;
                geo.Email_Hash = null;
                db.SaveChanges();

                //SendMailAws("diego@nonmonkey.com", "User Confirmed", geo.Email);

                return RedirectToAction("Login");
            }

            return View();
        }

        //RESET PASSWORD
        [HttpGet]
        public ActionResult ShouldChangePassword(string username)
        {
            ViewBag.username = username;
            return View();
        }
        [HttpGet]
        public ActionResult ResetPasswordRequest(string username)
        {
            ResetPasswordRequestModel model = new ResetPasswordRequestModel();
            model.Username = username;

            return View(model);
        }
        [HttpPost]
        public ActionResult ResetPasswordRequest(ResetPasswordRequestModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            user user = db.users.FirstOrDefault(x => x.UserName == model.Username || x.Email == model.Username );

            if (user == null)
            {
                ModelState.AddModelError(string.Empty, "User not found");
                return View(model);
            }

            string key = computeHash(DateTime.Now.ToString("dd/MM/yyHHmmss") + model.Username);
            user.Reset = true;
            user.ResetEndDate = DateTime.Now.AddDays(3);
            user.Email_Hash = key;

            TempData["success"] = "Check your inbox for the reset link";

            string url = Url.Action("ResetPassword", "Account", new { key = key }, "http");
            string title = "Conferma Reset Password";
            string body = string.Format("Ciao \r\n\r\nTo reset your password you should open the following link:\r\n\r\n{0}\r\n\r\nOr manually input the following code;\r\n\r\n \r\ncode:{1}", url, key);

            bool sent = SendMailAws(user.Email, title, body);
            if (sent)
            {
                db.SaveChanges();
                return RedirectToAction("ResetPassword");
            }
            else
            {
                ModelState.AddModelError("username", "There was an error sending the email");
                return View(model);

                //db.SaveChanges();
                //return RedirectToAction("ResetPassword", new { });
            }

            FormsAuthentication.SignOut();
            return RedirectToAction("ResetPassword");
        }
        [HttpGet]
        public ActionResult ResetPassword(string key)
        {
            ResetPasswordModel model = new ResetPasswordModel { Key = key };
            return View(model);
        }
        [HttpPost]
        public ActionResult ResetPassword(ResetPasswordModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            if (string.IsNullOrEmpty(model.Key))
            {
                ModelState.AddModelError("Password", "Wrong code, Please try again resetting your password");
                return View(model);
            }

            if (model.Password != model.Confirm)
            {
                ModelState.AddModelError("Confirm", "The passwords do not match");
                return View(model);
            }

            user geo = db.users.FirstOrDefault(x => x.Reset && x.Email_Hash == model.Key && x.ResetEndDate.HasValue && x.ResetEndDate.Value >= DateTime.Now);
            if (geo == null)
            {
                ModelState.AddModelError("Password", "Old reset code, please try again requesting a passowrd reset");
                return View(model);
            }


            geo.Reset = false;
            geo.ShouldChangePassword = false;
            geo.ResetEndDate = null;
            geo.UserPassword = computeHash(model.Password.ToUpper());
            geo.Email_Hash = null;

            db.SaveChanges();
            SendMailAws("diego@nonmonkey.com", "User Password Reset", geo.Email);

            return RedirectToAction("Login");
        }

        //DETAILS
        [HttpGet]
        [CustomAuthorize]
        public ActionResult UserSettings()
        {
            user user = db.users.FirstOrDefault(x => x.UserName == User.Identity.Name);
            SettingsModel model = new SettingsModel();

            model.Username = user.UserName;
            model.Bio = user.Bio;
            model.Name = user.Name;


            return View(model);
        }
        [HttpPost]
        [CustomAuthorize]
        public ActionResult UserSettings(SettingsModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            user user = db.users.FirstOrDefault(x => x.UserName == User.Identity.Name);

            if (!string.IsNullOrEmpty(model.OldPassword))
            {
                if (string.IsNullOrEmpty(model.Password) || string.IsNullOrEmpty(model.Confirm))
                {
                    ModelState.AddModelError("Password", "Specificare una nuova password");
                    return View(model);
                }
                if (model.Password != model.Confirm)
                {
                    ModelState.AddModelError("Confirm", "Le password non coincidono");
                    return View(model);
                }
                if (model.Password == model.OldPassword)
                {
                    ModelState.AddModelError("Password", "Devi specificare una password divera dalla precedente.");
                    return View(model);
                }


                string sha = computeHash(model.OldPassword);
                if (sha != user.UserPassword && (model.OldPassword != user.UserPassword && user.UserPassword.Length <= 10))
                {
                    ModelState.AddModelError("OldPassword", "La password non è corretta");
                    return View(model);
                }

                user.UserPassword = computeHash(model.Password);
            }

            user.Name = model.Name;
            user.Bio = model.Bio;
            db.SaveChanges();

            TempData["success"] = "Data updated";

            return RedirectToAction("Index");
        }

    }



}
