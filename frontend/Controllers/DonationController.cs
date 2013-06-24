using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Web;
using System.Web.Mvc;
using Localactors.entities;

namespace Localactors.webapp.Controllers
{

    /*
                mc_gross=10.00
                protection_eligibility=Eligible
                address_status=confirmed
                payer_id=JK9XPSEQK26MQ
                tax=0.00

                address_street=1+Main+St
                payment_date=02%3a20%3a39+Jun+05%2c+2013+PDT
                payment_status=Pending
                charset=windows-1252
                address_zip=95131
                first_name=tester
                address_country_code=US
                address_name=tester+localactors
                notify_version=3.7
                custom=
                payer_status=verified
                business=vespassassina-facilitator%40hotmail.com 
                address_country=United+States
                address_city=San+Jose
                quantity=0
                verify_sign=AV.-0arBFLRjhtG6WxCBZG1tVOgkAy--kpHG59KHFV6g3FffxZw68KAC
             * 
                payer_email=test%40localactors.org 
                txn_id=4AC54559D95373611
                payment_type=instant
                last_name=localactors
                address_state=CA
                receiver_email=vespassassina-facilitator%40hotmail.com 
             * 
                receiver_id=8R62TKM9TXLMY
                pending_reason=multi_currency
                txn_type=web_accept
                item_name=Project
             * 
                mc_currency=EUR
                item_number=
                residence_country=US
                test_ipn=1
                transaction_subject=Project
                payment_gross=
                ipn_track_id=883e6991b5453  
             */


    public class DonationController : ControllerBase
    {
        [Authorize(Roles = "supporter,admin,publisher")]
        public ActionResult Index() {
            var donations = db.donations.Include("project").Include("user").Where(x => x.user.UserName == User.Identity.Name);
            return View(donations.ToList());
        }

        [Authorize(Roles = "supporter,admin,publisher")]
        public ActionResult Return(int projectid=0)
        {

            LogStuff("PAYPAL RETURN", DateTime.Now, Request.QueryString.ToString() + "\r\n" + Request.Form.ToString());

            /*
             * mc_gross=1.00
             * &protection_eligibility=Eligible
             * &address_status=confirmed
             * &payer_id=JK9XPSEQK26MQ
             * &tax=0.00
             * &address_street=1+Main+St
             * &payment_date=07%3A06%3A59+Jun+05%2C+2013+PDT
             * &payment_status=Completed
             * &charset=windows-1252
             * &address_zip=95131
             * &first_name=tester
             * &mc_fee=0.38
             * &address_country_code=US
             * &address_name=tester+localactors
             * &notify_version=3.7
             * &custom=admin
             * &payer_status=verified
             * &business=vespassassina-facilitator%40hotmail.com
             * &address_country=United+States
             * &address_city=San+Jose
             * &quantity=0
             * &payer_email=test%40localactors.org
             * &verify_sign=An5ns1Kso7MWUdW4ErQKJJJ4qi4-A5uKSmudPHg6Bssh--AZqtYMP7Wh&txn_id=1SH19721FS9241319
             * &payment_type=instant
             * &last_name=localactors
             * &address_state=CA
             * &receiver_email=vespassassina-facilitator%40hotmail.com
             * &payment_fee=
             * &receiver_id=8R62TKM9TXLMY
             * &txn_type=web_accept
             * &item_name=Project
             * &mc_currency=EUR
             * &item_number=1
             * &residence_country=US
             * &test_ipn=1
             * &transaction_subject=admin
             * &payment_gross=
             * &merchant_return_link=Return+to+Diego+Trinciarelli's+Test+Store
             * &auth=AjhU-C2bLdsvWYwJHJhvzSRc08zgx9u3L231oxJdcHrFevcwbGsMj4MJVn9gc1WK8SdRTbimDZHXAkuD-gpm.VQ#.Ua9GDJxZL_c
            */

            string payer_id = Request["payer_id"];
            string payment_date = Request["payment_date"];

            transaction t = db.transactions.Include("Donation").FirstOrDefault(x => x.TransactionDate == payment_date && x.SenderID == payer_id);
            DonationDetails donation = new DonationDetails();
            if(t!=null && t.donation.user.UserName.ToLower() == User.Identity.Name.ToLower()) {
                donation.InvestmentID = t.donation.InvestmentID;
                donation.Amount = t.donation.Amount;
                donation.Currency = t.donation.Currency;
                donation.Description = t.donation.Description;
                ViewBag.projectid = projectid;
                return View(donation);
            }

            return RedirectToAction("ThankYou","Projects",new{id=projectid});
        }

        [HttpPost]
        [Authorize(Roles = "supporter,admin,publisher")]
        public ActionResult Return(DonationDetails model,int projectid=0)
        {

            donation donation = db.donations.FirstOrDefault(x => x.InvestmentID == model.InvestmentID && x.user.UserName.ToLower() == User.Identity.Name.ToLower());
            if(donation!=null) {
                donation.Description = model.Description;
                db.SaveChanges();
            }

            if(projectid>0) {
                return RedirectToAction("ThankYou", "Projects", new { id = projectid });
            }

            return RedirectToAction("Index", "Projects");
        }



        public ActionResult Callback() {

            //logging stuff
            string body = "";
            string nl = "\r\n";


            try {

                body = Request.Form.ToString().Replace("&", nl) + nl + nl;
                LogStuff("PAYPAL CALLBACK", DateTime.Now, Request.Form.ToString());

                string custom = Request.Form["custom"];
                string item_name = Request.Form["item_name"];
                string item_number = Request.Form["item_number"];

                string mc_gross = Request.Form["mc_gross"];
                string mc_currency = Request.Form["mc_currency"];
                string tax = Request.Form["tax"];

                string protection_eligibility = Request.Form["protection_eligibility"];
                string payer_id = Request.Form["payer_id"];
                string payer_status = Request.Form["payer_status"];
                string payer_email = Request.Form["payer_email"];

                string receiver_email = Request.Form["receiver_email"];
                string receiver_id = Request.Form["receiver_id"];

                string payment_status = Request.Form["payment_status"];
                string payment_date = Request.Form["payment_date"];
                string payment_gross = Request.Form["payment_gross"];

                string verify_sign = Request.Form["verify_sign"];
                string txn_id = Request.Form["txn_id"];
                string ipn_track_id = Request.Form["ipn_track_id"];
                string pending_reason = Request.Form["pending_reason"];

                string status = Verify();

                //1: check duplicate transaction
                var duplicate = db.transactions.FirstOrDefault(x => x.TransactionCode == txn_id);
                if (duplicate != null) {
                    SendMailAws(ConfigurationManager.AppSettings["PP_webappEmailNotificationAddress"], "Paypal IPN", "Duplicate Transaction: " + body);
                    return Content("Duplicate");
                }

                //2:convert data
                int projectid = 0;
                int.TryParse(item_number, out projectid);
                decimal amount = 0;
                decimal.TryParse(mc_gross, out amount);
                decimal taxamount = 0;
                decimal.TryParse(tax, out taxamount);

                //user
                user user = db.users.FirstOrDefault(x => x.UserName == custom);
                //project
                project project = db.projects.FirstOrDefault(x => x.ProjectID == projectid);

                //3: create donation
                donation d = new donation();
                db.donations.AddObject(d);
                if (projectid > 0)
                    d.ProjectID = projectid;
                d.Amount = amount/100;
                d.Currency = mc_currency;
                d.Date = DateTime.Now;
                d.Description = "";
                d.UserID = user.UserID;

                d.PP_Amount = mc_gross;
                d.PP_Currency = mc_currency;
                d.PP_Email = payer_email;
                d.PP_Status = status;
                d.PP_Transaction = txn_id;

                //4: save transaction
                transaction t = new transaction();
                d.transactions.Add(t);
                t.Currency = mc_currency;
                t.GrossAmount = amount / 100;
                t.ItemName = item_name;
                t.ItemNumber = item_number;
                t.PaymentStatus = payment_status;
                t.PendingReason = pending_reason;
                t.ReceiverEmail = receiver_email;
                t.ReceiverID = receiver_id;
                t.SenderEmail = payer_email;
                t.SenderID = payer_id;
                t.TaxAmount = taxamount / 100;
                t.TransactionDate = payment_date;
                t.TransactionCode = txn_id;
                t.TransactionSignature = verify_sign;
                t.TransactionStatus = status;
                t.TransactionType = "PP";

                db.SaveChanges();

                string mailbody = string.Format("From: {0}\r\nName: {1}\r\nProject: {2}\r\nProjectID: {3}\r\n\r\nPaypal Donation Data: {4}", user.Email, user.UserName, project.Title, project.ProjectID, body);
                SendMailAws(ConfigurationManager.AppSettings["Email_Info"], "Donation: " + project.Title, mailbody);
                SendMailAws(project.user.Email, "Donation: " + project.Title, mailbody);
                SendMailAwsAdmin("Donation: " + project.Title, mailbody);
                SendMailAwsTemplate(user, project, user.Email, "donation_thankyou.htm", "Thank You!", "");
                

                //5: add follower
                try {
                    if (project != null && user!=null) {
                        //TODO: fix here!
                        var followed = user.followedProjects.FirstOrDefault(x => x.ProjectID == projectid);
                        if (followed == null) {
                            user.followedProjects.Add(project);
                            db.SaveChanges();
                        }
                    }
                }catch(Exception exi) {
                    SendMailAws(ConfigurationManager.AppSettings["PP_webappEmailNotificationAddress"], "DB Follow Error:", exi.Message + " // " + (exi.InnerException != null ? exi.InnerException.Message : ""));
                    LogStuff("DB FOLLOW ERROR", DateTime.Now, exi.Message + " // " + (exi.InnerException != null ? exi.InnerException.Message : ""));
                }

                return Content("");
                
            }catch(Exception ex) {
                SendMailAws(ConfigurationManager.AppSettings["PP_webappEmailNotificationAddress"], "Paypal Error:", ex.Message + " // " + (ex.InnerException != null ? ex.InnerException.Message : ""));
                LogStuff("PAYPAL ERROR", DateTime.Now, ex.Message + " // " + (ex.InnerException != null ? ex.InnerException.Message : ""));
                LogStuff("PAYPAL ERROR", DateTime.Now, body);
                return Content("");
            }
        }

        private string Verify() {

            try {
                string endpoint = ConfigurationManager.AppSettings["PP_IPNEndpoint"];
                HttpWebRequest req = (HttpWebRequest) WebRequest.Create(endpoint);

                req.Method = "POST";
                req.ContentType = "application/x-www-form-urlencoded";
                byte[] param = Request.BinaryRead(Request.ContentLength);
                string strRequest = Encoding.ASCII.GetString(param);
                string strResponse_copy = strRequest; //Save a copy of the initial info sent by PayPal
                strRequest += "&cmd=_notify-validate";
                req.ContentLength = strRequest.Length;

                //for proxy
                //WebProxy proxy = new WebProxy(new Uri("http://url:port#"));
                //req.Proxy = proxy;

                //Send the request to PayPal and get the response
                StreamWriter streamOut = new StreamWriter(req.GetRequestStream(), Encoding.ASCII);
                streamOut.Write(strRequest);
                streamOut.Close();
                StreamReader streamIn = new StreamReader(req.GetResponse().GetResponseStream());
                string strResponse = streamIn.ReadToEnd();
                streamIn.Close();

                if (strResponse == "VERIFIED") {
                    return "VERIFIED";
                    //check the payment_status is Completed
                    //process payment

                    // pull the values passed on the initial message from PayPal

                }
                else if (strResponse == "INVALID") {
                    return "INVALID";
                    //log for manual investigation
                }
                else {
                    //log response/ipn data for manual investigation
                    return "PENDING";
                }
            }catch(Exception ex) {
                SendMailAws(ConfigurationManager.AppSettings["PP_webappEmailNotificationAddress"], "Paypal Verification Error:", ex.Message);
                LogStuff("PAYPAL VERIFICATION ERROR", DateTime.Now, ex.Message);
                return "PENDING";
            }
        }

    }
}
