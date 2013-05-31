using System;
using System.Web;
using System.Web.Mvc;
using Newtonsoft.Json;


    public class CustomJsonResult : JsonResult
    {
        public CustomJsonResult()
        {
            JsonRequestBehavior = JsonRequestBehavior.AllowGet;
        }

        public override void ExecuteResult(ControllerContext context)
        {
            if (context == null)
                throw new ArgumentNullException("context");
            

            if (JsonRequestBehavior == JsonRequestBehavior.DenyGet && String.Equals(context.HttpContext.Request.HttpMethod, "GET", StringComparison.OrdinalIgnoreCase))
                throw new InvalidOperationException("Get Not Allowed");
            
            HttpResponseBase response = context.HttpContext.Response;
            response.ContentType = !String.IsNullOrEmpty(ContentType) ? ContentType : "application/json";
            response.AddHeader("Access-Control-Allow-Origin","*");

            if (ContentEncoding != null)
                response.ContentEncoding = ContentEncoding;

            if (Data != null)
                response.Write(JsonConvert.SerializeObject(Data, Formatting.None));
        }
    }


