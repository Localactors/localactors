using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO.Compression;
using System.Linq;
using System.Web;
using System.Web.Mvc;



    public class CompressFilter : ActionFilterAttribute
    {
        public override void  OnActionExecuting(ActionExecutingContext filterContext)
        {
            if(ConfigurationManager.AppSettings["compress"]==null)
                return;

            HttpRequestBase request = filterContext.HttpContext.Request;

            string acceptEncoding = request.Headers["Accept-Encoding"];

            if (string.IsNullOrEmpty(acceptEncoding) || filterContext.IsChildAction) return;

           

            acceptEncoding = acceptEncoding.ToUpperInvariant();

            HttpResponseBase response = filterContext.HttpContext.Response;

            if (acceptEncoding.Contains("GZIP"))
            {
                response.AppendHeader("Content-encoding", "gzip");
                response.Filter = new GZipStream(response.Filter, CompressionMode.Compress);
            }
            else if (acceptEncoding.Contains("DEFLATE"))
            {
                response.AppendHeader("Content-encoding", "deflate");
                response.Filter = new DeflateStream(response.Filter, CompressionMode.Compress);
            }
        }
    }
