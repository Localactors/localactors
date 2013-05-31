using System.ServiceModel.Syndication;
using System.Web.Mvc;
using System.Xml;


    public class RssFeedResult : ActionResult
    {

        public RssFeedResult()
        {
        }

        public RssFeedResult(SyndicationFeed feed)
        {
            Feed = feed;
        }

        public SyndicationFeed Feed
        {
            get;
            set;
        }


        public override void ExecuteResult(ControllerContext context)
        {
            context.HttpContext.Response.ContentType = "application/rss+xml";
            Rss20FeedFormatter rssFormatter = new Rss20FeedFormatter(Feed);
            using (XmlWriter writer = XmlWriter.Create(context.HttpContext.Response.Output))
            {
                rssFormatter.WriteTo(writer);
            }
            context.HttpContext.Response.Flush();
        }
    }