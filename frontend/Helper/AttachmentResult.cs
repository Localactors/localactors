using System;
using System.Web.Mvc;

    public class AttachmentResult : ActionResult
    {

        public AttachmentResult()
        {
        }

        public AttachmentResult(string content, string filename, string mimetype) {
            Mimetype = mimetype;
            Content = content;
            Filename = filename;
        }

        public string Content
        {
            get;
            set;
        }

        public string Mimetype
        {
            get;
            set;
        }

        public string Filename
        {
            get;
            set;
        }

        public override void ExecuteResult(ControllerContext context) {
            if (!String.IsNullOrEmpty(Filename)) {
                context.HttpContext.Response.AddHeader("content-disposition", "attachment; filename=" + Filename);
                context.HttpContext.Response.ContentType = Mimetype;
            }

            context.HttpContext.Response.Write(Content);
            context.HttpContext.Response.Flush();
        }
    }
