using System.Web.Mvc;

namespace Localactors.webapp.Areas.Publisher
{
    public class PublisherAreaRegistration : AreaRegistration
    {
        public override string AreaName
        {
            get
            {
                return "Publisher";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context)
        {

            context.MapRoute(
                "Publisher_default_home",
                "/Publisher",
                new {controller="home", action = "Index", id = UrlParameter.Optional }
           );

            context.MapRoute(
                "Publisher_default",
                "Publisher/{controller}/{action}/{id}",
                new { action = "Index", id = UrlParameter.Optional }
            );
        }
    }
}
