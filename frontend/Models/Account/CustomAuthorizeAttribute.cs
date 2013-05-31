using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace WSC_webapp.Models
{
    public class CustomAuthorizeAttribute : AuthorizeAttribute
    {
        public override void OnAuthorization(AuthorizationContext filterContext)
        {
            base.OnAuthorization(filterContext);
            if (filterContext.Result is HttpUnauthorizedResult)
            {
                filterContext.Result = new RedirectResult(System.Web.Security.FormsAuthentication.LoginUrl +
                     "?ReturnUrl=" +
                     filterContext.HttpContext.Server.UrlEncode(filterContext.HttpContext.Request.RawUrl));
            }
        }

    }

}