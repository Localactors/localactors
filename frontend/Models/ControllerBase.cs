using System;
using System.Collections.Generic;
using System.Configuration;
using System.Drawing;
using System.Dynamic;
using System.Globalization;
using System.IO;
using System.Net;
using System.Net.Mail;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using System.Web.Caching;
using System.Web.Mvc;
using System.Web.Script.Serialization;
using Localactors.entities;
using Amazon.S3;
using Amazon.S3.Model;
using NLog;
using System.Linq;

namespace Localactors.webapp
{
    [HandleError]
    public class ControllerBase : Controller
    {

        internal readonly localactors db = new localactors();
        internal  user CurrentUser {
            get {
                string username = User.Identity.Name;
                if (username == null) {
                    return new user(){UserID = 0,UserName = null,Email = null};
                }

                user user = System.Web.HttpContext.Current.Cache.Get(username) as user;
                if(user==null) {
                    user = db.users.FirstOrDefault(x => x.UserName.ToLower() == User.Identity.Name.ToLower());
                    if (user != null) {
                        System.Web.HttpContext.Current.Cache.Insert(username, user, null, DateTime.Now.AddMinutes(10), Cache.NoSlidingExpiration, CacheItemPriority.Normal, null);
                    }
                }

                return user;
            }
        }
        Logger log = LogManager.GetCurrentClassLogger();


        protected override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            base.OnActionExecuting(filterContext);
        }
        protected override void OnException(ExceptionContext filterContext)
        {
            base.OnException(filterContext);
            LogAppend("controller", 'E', filterContext.Exception.Message);

        }

        //URLS
        protected string AddVariableToUrl(string url, string name, string value)
        {
            url = url + (url.IndexOf("?") > 0 ? "&" : "?") + name + "=" + value;
            return url;
        }
        protected string ReferrerUrlTimestamped() {
            string referrer = Request.UrlReferrer != null ? Request.UrlReferrer.AbsolutePath : "/";
            string url = AddVariableToUrl(referrer, "tsp", DateTime.Now.Ticks.ToString());
            return url;
        }
        protected string CurrentUrlTimestamped()
        {
            string current = Request.Url != null ? Request.Url.AbsolutePath : "/";
            string url = AddVariableToUrl(current, "tsp", DateTime.Now.Ticks.ToString());
            return url;
        }
        protected ActionResult RedirectToAction(string action, string controller, object routeValues, string anchor) {
            return new RedirectResult(Url.Action(action,controller,routeValues) + "#" + anchor);
        }

        //Transaction Logs
        protected void LogStuff(string type, DateTime date, string body) {
            try {
                transaction_dump dump = new transaction_dump();
                dump.Date = date;
                dump.Type = type;
                dump.Dump = body;

                db.transaction_dump.AddObject(dump);
                db.SaveChanges();
            }catch(Exception ex) {
                //do nothing
            }
        }

        //JSON
        protected static T DeserializeFromJson<T>(string input)
        {
            JavaScriptSerializer serializer = new JavaScriptSerializer();
            return serializer.Deserialize<T>(input);
        }
        protected static string SerializeToJson(object input)
        {
            JavaScriptSerializer serializer = new JavaScriptSerializer();
            return serializer.Serialize(input);
        }
        protected new internal JsonResult Json(object data)
        {
            return new CustomJsonResult { Data = data };
        }

        //arrays
        protected string ListToString(List<int> list ) {
            return string.Join(";",list);
        }
        protected List<int> StringToList(string list)
        {
            List<int> ints = new List<int>();
            var arr = list.Split(new []{';'});
            foreach (string s in arr) {
                int tmp = 0;
                if(int.TryParse(s,out tmp)) {
                    ints.Add(tmp);
                }
            }
            return ints;
        }

       //sanitize
        protected string sanitizeHtml(string html) {
            return WebUtility.HtmlEncode(html);
        }

        //user cookies
        protected void PushCookies(user user)
        {
            ////remove
            //if(user == null) {
            //    HttpCookie tc = new HttpCookie("team");
            //    tc.Expires = DateTime.Now.AddYears(-1);
            //    Response.Cookies.Add(tc);
            //    return;
            //}

            //userteam team = user.userteam;
            //if (team != null)
            //{
            //    HttpCookie teamcookie = new HttpCookie("team");
            //    teamcookie.Values.Add("name", user.userteam.Name);
            //    teamcookie.Values.Add("logo", user.userteam.Avatar);
            //    teamcookie.Values.Add("motto", user.userteam.Motto);
            //    teamcookie.Expires = DateTime.Now.AddYears(1);
            //    Response.Cookies.Add(teamcookie);
            //}

        }

        //hash
        internal string computeHash(string input)
        {
            var algorithm = SHA256.Create();
            Byte[] inputBytes = Encoding.UTF8.GetBytes(input);
            Byte[] hashedBytes = algorithm.ComputeHash(inputBytes);
            string H = BitConverter.ToString(hashedBytes).Replace("-", "");
            return H;
        }
        internal string getGuid() {
            return Guid.NewGuid().ToString("N");
        }
        internal string getTimestamp() {
            return DateTime.Now.Ticks.ToString();
        }

        //IMAGES
        protected Image DownloadImage(string _URL)
        {
            Image _tmpImage = null;

            try
            {
                var _HttpWebRequest = (HttpWebRequest)WebRequest.Create(_URL);
                _HttpWebRequest.AllowWriteStreamBuffering = true;
                _HttpWebRequest.UserAgent = "Mozilla/4.0 (compatible; MSIE 7.0; Windows NT 5.1)";
                _HttpWebRequest.Referer = "http://www.google.com/";
                _HttpWebRequest.Timeout = 200000;

                WebResponse _WebResponse = _HttpWebRequest.GetResponse();
                Stream _WebStream = _WebResponse.GetResponseStream();
                _tmpImage = Image.FromStream(_WebStream);

                _WebResponse.Close();
                _WebResponse.Close();
            }
            catch (Exception _Exception)
            {
                return null;
            }

            return _tmpImage;
        }
        internal Image ResizeImage(int width, int height, Image image)
        {
            Image resized = image.Resize(height, width);
            return resized;
        }
        internal string ResizeImage(int width, int height, string sourcePath, string resizedPath)
        {
            if (System.IO.File.Exists(sourcePath))
            {
                using (Image ImageInfo = Image.FromFile(sourcePath))
                {
                    Image resized = ImageInfo.Resize(height, width);
                    resized.Save(resizedPath);
                    return resizedPath;
                }
            }
            return null;
        }
        internal string ResizeCropImage(int width, int height, string sourcePath, string resizedPath)
        {
            if (System.IO.File.Exists(sourcePath))
            {
                using (Image ImageInfo = Image.FromFile(sourcePath))
                {
                    Image resized = ImageHelper.HardResizeImage(width, height, ImageInfo);

                    if (System.IO.File.Exists(resizedPath))
                        System.IO.File.Delete(resizedPath);

                    resized.Save(resizedPath);
                    return resizedPath;
                }
            }
            return null;
        }

        //Email
        internal bool SendMail(string to, string title, string body)
        {
            try
            {


                var client = new SmtpClient
                    {
                        Host = ConfigurationManager.AppSettings["mail_server"],
                        Port = int.Parse(ConfigurationManager.AppSettings["mail_port"]),
                        UseDefaultCredentials = true
                    };

                if (!string.IsNullOrEmpty(ConfigurationManager.AppSettings["mail_user"]))
                    client.Credentials = new NetworkCredential(ConfigurationManager.AppSettings["mail_user"], ConfigurationManager.AppSettings["mail_password"]);


                bool ssl = false;
                bool.TryParse(ConfigurationManager.AppSettings["mail_ssl"], out ssl);
                client.EnableSsl = ssl;

                client.Send(ConfigurationManager.AppSettings["mail_from"], to, title, body);


                return true;
            }
            catch (Exception ex)
            {
                string msg = ex.Message;
                LogAppend("sendmail", 'E', msg);
                return false;
            }
        }
        internal bool SendMailAws(string to, string title, string body)
        {
            try
            {

                var client = new SmtpClient
                {
                    Host = ConfigurationManager.AppSettings["AWS_smtp_host"],
                    Port = int.Parse(ConfigurationManager.AppSettings["AWS_smtp_port"]),
                    UseDefaultCredentials = true
                };

                client.Credentials = new NetworkCredential(
                    ConfigurationManager.AppSettings["AWS_smtp_user"],
                    ConfigurationManager.AppSettings["AWS_smtp_pass"]);
                client.EnableSsl = true;
                client.Send(ConfigurationManager.AppSettings["AWS_mailfrom"], to, title, body);


                return true;
            }
            catch (Exception ex)
            {
                string msg = ex.Message;
                LogAppend("sendmail", 'E', msg);
                return false;
            }
        }

        //dates
        internal DateTime DateFromString(string date)
        {
            DateTime convertedDate;
            if (!DateTime.TryParseExact(date, "ddMMyyyyHHmmss", new CultureInfo("it-IT"), DateTimeStyles.None, out convertedDate))
                throw new FormatException(string.Format("Unable to format date:{0}", date));

            return convertedDate;
        }
        internal string DateToString(DateTime date)
        {
            return date.ToString("ddMMyyyyHHmmss");
        }

        //logging
        internal void LogAppend(string source = "", char logType = 'E', string logMessage = "")
        {
            log.Error(logMessage);
            return;


            try
            {
                if (!string.IsNullOrEmpty(source))
                {
                    if (source.Length > 50) source = source.Substring(0, 50);
                }
                else
                {
                    source = "application";
                }



                //log l = new log() { LogDate = DateTime.Now, LogSource = source.ToUpper(), LogType = logType.ToString().ToUpper(), LogMessage = logMessage };
                //db.logs.AddObject(l);
                //db.SaveChanges();
            }
            catch { }
        }

        //S3 UTILS
        protected AmazonS3 AmazonS3Client
        {
            get {
                AmazonS3Config config = new AmazonS3Config();
                config.ServiceURL = ConfigurationManager.AppSettings["AWSEUEndPoint"];

                AmazonS3 s3Client = Amazon.AWSClientFactory.CreateAmazonS3Client(ConfigurationManager.AppSettings["AWSAccessKey"], ConfigurationManager.AppSettings["AWSSecretKey"], config);

                return s3Client;
            }
        }
        protected JsonResult S3List(string foldername)
        {
            //StringBuilder sb = new StringBuilder();

            // get the objects at the TOP LEVEL, i.e. not inside any folders
            AmazonS3Client s3Client = new AmazonS3Client();
            string folder = foldername;
            var request = new ListObjectsRequest().WithBucketName(ConfigurationManager.AppSettings["AWSS3Bucket"]).WithPrefix(folder);
            ListObjectsResponse response = s3Client.ListObjects(request);

            dynamic jsondata = new ExpandoObject();
            jsondata.bucket = ConfigurationManager.AppSettings["AWSS3Bucket"];
            jsondata.objects = new List<Object>();

            foreach (S3Object s3Object in response.S3Objects)
            {
                dynamic s3obj = new ExpandoObject();
                s3obj.fullpath = s3Object.Key;
                s3obj.localpath = s3Object.Key.Replace(folder, "");
                s3obj.size = s3Object.Size;
                s3obj.storage = s3Object.StorageClass;

                jsondata.objects.Add(s3obj);
            }
            return Json(jsondata, JsonRequestBehavior.AllowGet);
        }
        protected string S3UploadFile(string folder, string filename, Stream inputStream)
        {
            //https://s3-eu-west-1.amazonaws.com/Localactors.content/cute-welsh-corgi-puppy-400x300.jpg

            //ok, making the new path
            string filepath =  filename;
            if(!string.IsNullOrEmpty(folder))
                filepath = string.Format("{0}/{1}", folder, filename);

            //upload
            AmazonS3Client s3Client = new AmazonS3Client();
            var request = new PutObjectRequest()
                .WithBucketName(ConfigurationManager.AppSettings["AWSS3Bucket"])
                .WithKey(filepath);
            request.InputStream = inputStream;
            s3Client.PutObject(request);

            return filepath;

        }
        protected JsonResult S3UploadPostedFiles(string folder)
        {
            if (Request.Files != null && Request.Files.Count > 0)
            {
                foreach (string keyname in Request.Files)
                {
                    HttpPostedFileBase file = Request.Files[keyname];
                    if (file.ContentLength > 0)
                    {
                        //ok, making the new filename
                        string filepath =  file.FileName;
                        if(!string.IsNullOrEmpty(folder))
                            filepath = string.Format("{0}/{1}", folder, file.FileName);

                        //upload
                        AmazonS3Client s3Client = new AmazonS3Client();
                        var request = new PutObjectRequest()
                            .WithBucketName(ConfigurationManager.AppSettings["AWSS3Bucket"])
                            .WithKey(filepath);
                        request.InputStream = file.InputStream;
                        s3Client.PutObject(request);
                    }
                }
            }
            return Json(new { result = "OK" });
        }
        protected JsonResult S3Delete(string path)
        {
            AmazonS3Client s3Client = new AmazonS3Client();
            var request = new DeleteObjectRequest()
                .WithBucketName(ConfigurationManager.AppSettings["AWSS3Bucket"])
                .WithKey(path);
            s3Client.DeleteObject(request);

            return Json(new { result = "OK" });
        }

        
    }
}
