using System;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using Localactors.entities;


namespace Localactors.webapp.Areas.Admin.Controllers
{

    public class AccountController : ControllerBase
    {


        [Authorize(Roles = "admin")]
        public ActionResult Index()
        {
            Response.Cache.SetCacheability(HttpCacheability.NoCache);

            if (!Request.IsAuthenticated)
                return RedirectToAction("Login", "Account");

            user user = db.users.FirstOrDefault(x => x.UserName == User.Identity.Name);
            if(user==null)
                return RedirectToAction("Login", "Account");

            return View();
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
        public ActionResult Login(LoginModel model) {

            Response.Cache.SetCacheability(HttpCacheability.NoCache);

            if (!ModelState.IsValid)
                return View(model);

            user a = db.users.FirstOrDefault(x => x.UserName == model.Username);

            if (a == null) {
                ModelState.AddModelError("Username","Utente o Password errati");
                return View(model);
            }


            if(a.Enabled == false ){
                ModelState.AddModelError("Username","Utente non abilitato");
                return View(model);
            }

            string sha = computeHash(model.Password);
            if (a.UserPassword == sha || (a.UserPassword.Length < 10 && a.UserPassword == model.Password))
            {
                FormsAuthentication.SetAuthCookie(model.Username, model.RememberMe);
                if (!string.IsNullOrEmpty(model.ReturnUrl))
                    return Redirect(model.ReturnUrl);
                else
                    return RedirectToAction("Index", "Home");
            }



            //fallback out!
            TempData["error"] = "Errore di accesso";
            ModelState.AddModelError("Username", "Utente o Password errati");
            return View(model);
        }

        [HttpGet]
        public ActionResult Logout()
        {
            Response.Cache.SetCacheability(HttpCacheability.NoCache);

            FormsAuthentication.SignOut();
            return RedirectToAction("Login", "Account", new { area = "Admin" });
        }

        [HttpGet]
        public ActionResult ChangePassword() {
            return View(new ChangePasswordModel());
        }

        [HttpPost]
        public ActionResult ChangePassword(ChangePasswordModel model) {

            string oldhash = computeHash(model.oldpassword);
            string newhash = computeHash(model.newpassword);
            string nameash = computeHash(User.Identity.Name);

            user user = db.users.FirstOrDefault(x => x.UserName == User.Identity.Name);
            if (user.UserPassword == oldhash || (user.UserPassword.Length < 10 && user.UserPassword == model.oldpassword))
            {
                if (model.newpassword == model.confirm)
                {
                    user.UserPassword = newhash;
                    db.SaveChanges();
                    return RedirectToAction("Index", "Home", new { area = "Admin" });
                }else {
                    ModelState.AddModelError("confirm","The confirmation password should match with the new password.");
                }
            }else {
                ModelState.AddModelError("oldpassword","Wrong Password");
            }
            return View(model);
        }

    }

    

}
