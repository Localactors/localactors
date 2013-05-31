using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

public static class HtmlHelperExtensions {
    public static HtmlString CookieValue(this HtmlHelper helper, string cookie, string valuekey, string emptyplaceholder)
    {
        string cookievalue = emptyplaceholder;

        HttpCookie myCookie = HttpContext.Current.Request.Cookies[cookie];
        if (myCookie != null)
        {
            if (!string.IsNullOrEmpty(myCookie.Values[valuekey]))
            {
                cookievalue = myCookie.Values[valuekey].ToString();
            }
        }


        return new HtmlString(cookievalue);
    }

    public static HtmlString Pager(this HtmlHelper helper, int curpage, int totalpages, int showpages) {
        int delta = showpages;

        var urlHelper = new UrlHelper(helper.ViewContext.RequestContext, helper.RouteCollection);
        var currentControllerName = (string) helper.ViewContext.RouteData.Values["controller"];
        var currentActionName = (string) helper.ViewContext.RouteData.Values["action"];
        var currentSort = (string) helper.ViewContext.RouteData.Values["sort"];
        //var currentCity = (string)helper.ViewContext.RouteData.Values["city"];

        var routeValueDict = new RouteValueDictionary();
        foreach (string name in helper.ViewContext.RouteData.Values.Keys) {
            routeValueDict.Add(name, helper.ViewContext.RouteData.Values[name]);
        }


        int min = curpage - delta;
        int max = curpage + delta;
        if (min < 1) min = 1;
        if (max > totalpages) max = totalpages;

        var sb = new StringBuilder();
        sb.Append("<div class='pager'>");

        if (curpage > 1) {
            sb.AppendFormat("<a href='{0}' class='prev button'>Prima</a>", urlHelper.RouteUrl(new { Controller = currentControllerName, Action = currentActionName, page = 1, sort = currentSort }));
            sb.AppendFormat("<a href='{0}' class='prev button'>Precedente</a>", urlHelper.RouteUrl(new { Controller = currentControllerName, Action = currentActionName, page = curpage - 1, sort = currentSort }));
        }

        if (min < max) {
            for (int i = min; i <= max; i++) {
                if (i != curpage) {
                    string url = urlHelper.RouteUrl(new { Controller = currentControllerName, Action = currentActionName, page = i, sort = currentSort });
                    sb.AppendFormat("<a href='{0}' class='pagenumber button'>{1}</a>", url, i);
                }
                else {
                    sb.AppendFormat("<a class='pagecurrent button yellow'>{0}</a>", i);
                }
            }
        }

        if (curpage < totalpages) {
            sb.AppendFormat("<a href='{0}' class='next button'>Successiva</a>", urlHelper.RouteUrl(new { Controller = currentControllerName, Action = currentActionName, page = curpage + 1, sort = currentSort }));
            sb.AppendFormat("<a href='{0}' class='next button'>Ultima</a>", urlHelper.RouteUrl(new { Controller = currentControllerName, Action = currentActionName, page = totalpages, sort = currentSort }));
        }

        sb.Append("</div>");

        return new HtmlString(sb.ToString());
    }

    public static HtmlString ImageActionLink(this HtmlHelper helper, string imageUrl, string actionName, string controllerName = null, string altText = null, string rightText = null, object routeValues = null, object linkHtmlAttributes = null, object imgHtmlAttributes = null) {
        var urlHelper = new UrlHelper(helper.ViewContext.RequestContext, helper.RouteCollection);
        string imgurl = urlHelper.Content(imageUrl);

        var imgBuilder = new TagBuilder("img");
        imgBuilder.MergeAttribute("src", imgurl);
        if (imgHtmlAttributes != null) {
            Dictionary<string, object> imgAttributes = AnonymousObjectToKeyValue(imgHtmlAttributes);
            imgBuilder.MergeAttributes(imgAttributes, true);
        }
        if (!string.IsNullOrEmpty(altText))
            imgBuilder.MergeAttribute("alt", altText);

        var linkBuilder = new TagBuilder("a");
        linkBuilder.MergeAttribute("href", urlHelper.Action(actionName, controllerName, routeValues));
        if (linkHtmlAttributes != null) {
            Dictionary<string, object> linkAttributes = AnonymousObjectToKeyValue(linkHtmlAttributes);
            linkBuilder.MergeAttributes(linkAttributes, true);
        }

        if (rightText == null)
            rightText = "";

        string text = string.Format("{0}{1}{3}{2}", linkBuilder.ToString(TagRenderMode.StartTag), imgBuilder.ToString(TagRenderMode.SelfClosing), linkBuilder.ToString(TagRenderMode.EndTag), rightText);

        return new HtmlString(text);
    }

    public static HtmlString RepeatingChars(this HtmlHelper helper, string repeat, int count) {
        var sb = new StringBuilder();
        for (int i = 0; i < count; i++) {
            sb.AppendLine(repeat);
        }
        return new HtmlString(sb.ToString());
    }

    public static HtmlString RepeatingChars(this HtmlHelper helper, string repeat, float count) {
        var sb = new StringBuilder();
        for (int i = 0; i < count; i++) {
            sb.AppendLine(repeat);
        }
        return new HtmlString(sb.ToString());
    }

    public static HtmlString Truncate(this HtmlHelper helper, string stringtotruncate, int maxlen, string ellipsis) {
        if (ellipsis == null) ellipsis = "";
        maxlen = maxlen - ellipsis.Length;

        if (stringtotruncate==null || stringtotruncate.Length <= maxlen) 
            return new HtmlString(stringtotruncate);

        return new HtmlString(string.Format("{0}{1}",stringtotruncate.Substring(0,maxlen),ellipsis));
        
    }

    private static Dictionary<string, object> AnonymousObjectToKeyValue(object anonymousObject)
    {
        var dictionary = new Dictionary<string, object>();
        if (anonymousObject != null)
        {
            foreach (PropertyDescriptor propertyDescriptor in TypeDescriptor.GetProperties(anonymousObject))
            {
                dictionary.Add(propertyDescriptor.Name, propertyDescriptor.GetValue(anonymousObject));
            }
        }
        return dictionary;
    }

    public static string AbsoluteAction(this UrlHelper url, string action, string controller, object routeValues) {
        Uri requestUrl = url.RequestContext.HttpContext.Request.Url;
        string absoluteAction = string.Format("{0}{1}", requestUrl.GetLeftPart(UriPartial.Authority), url.Action(action, controller, routeValues));
        return absoluteAction;
    }
    public static IDictionary<string, string> GetModelStateErrors(this ViewDataDictionary viewDataDictionary)
    {
        Dictionary<string, string> dict = new Dictionary<string, string>();
        foreach (var modelStateKey in viewDataDictionary.ModelState.Keys)
        {
            var modelStateValue = viewDataDictionary.ModelState[modelStateKey];
            foreach (var error in modelStateValue.Errors)
            {
                var errorMessage = error.ErrorMessage;
                var exception = error.Exception;
                if (!String.IsNullOrEmpty(errorMessage))
                {
                    dict.Add(modelStateKey, "Egads! A Model Error Message! " + errorMessage);
                }
                if (exception != null)
                {
                    dict.Add(modelStateKey, "Egads! A Model Error Exception! " + exception.ToString());
                }
            }
        }
        return dict;
    }
}