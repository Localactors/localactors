using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using System.Web.Security;

namespace Localactors.webapp
{
    // Nota: per istruzioni su come abilitare la modalità classica di IIS6 o IIS7, 
    // visitare il sito Web all'indirizzo http://go.microsoft.com/?LinkId=9394801

    public class MvcApplication : System.Web.HttpApplication
    {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new HandleErrorAttribute());
        }

        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            routes.MapRoute(
                "LoginRedirect", // Nome route
                "login", // URL con parametri
                new { controller = "Home", action = "Login", id = UrlParameter.Optional }, // Valori predefiniti parametri
                new[] { "Localactors.webapp.Controllers" }
            );

            routes.MapRoute(
                "Default", // Nome route
                "{controller}/{action}/{id}", // URL con parametri
                new { controller = "Home", action = "index", id = UrlParameter.Optional }, // Valori predefiniti parametri
                new[] { "Localactors.webapp.Controllers" }
            );

            

        }

        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();

            RegisterGlobalFilters(GlobalFilters.Filters);
            RegisterRoutes(RouteTable.Routes);

	        DefaultModelBinder.ResourceClassKey = "MyResources";
        }

        protected void Application_AuthenticateRequest(Object sender, EventArgs e)
        {
            HttpCookie authCookie = Request.Cookies[FormsAuthentication.FormsCookieName];
            if (authCookie != null)
            {
                try
                {
                    FormsAuthenticationTicket authTicket = FormsAuthentication.Decrypt(authCookie.Value);
                    CustomIdentity identity = CustomIdentity.GetIdentity(authTicket.Name);
                    CustomPrincipal newUser = new CustomPrincipal(identity);
                    Context.User = newUser;
                }
                catch (Exception)
                {
                    FormsAuthentication.SignOut();
                }
            }
        }
    }
}