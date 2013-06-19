using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Caching;
using System.Text;
using System.Web;
using System.Web.Mvc;

namespace Localactors.webapp.Areas.Admin.Controllers
{

    public class HomeController : ControllerBase
    {
        //
        // GET: /Admin/Home/

        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Caches()
        {

            var cache = System.Web.HttpContext.Current.Cache;
            IDictionaryEnumerator enumerator = cache.GetEnumerator();

            StringBuilder sb = new StringBuilder();
            sb.AppendLine("----------------------------------------");
            sb.AppendLine("Standard");
            while (enumerator.MoveNext())
            {
                sb.AppendLine(enumerator.Key.ToString());
            }

            sb.AppendLine("----------------------------------------");
            sb.AppendLine("Child");
            var dict = ((System.Runtime.Caching.MemoryCache)OutputCacheAttribute.ChildActionCache).AsEnumerable();
            foreach (KeyValuePair<string, object> pair in dict) {
                sb.AppendLine(pair.Key);
            }

            return Content(sb.ToString());
        }
    }
}
