using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Neimart.Core.Entities;
using Neimart.Core.Utilities.Extensions;
using Neimart.Core.Utilities.Helpers;
using OpenGraphNet;

namespace Neimart.Core.Infrastructure.Web
{
    public static class HtmlExtensions
    {
        public static IHtmlContent GetThemeModeSuffix(this IHtmlHelper htmlHelper)
        {
            var theme = AsyncHelper.RunSync(() => htmlHelper.ViewContext.HttpContext.GetThemeAsync());

            var content = theme.Mode == ThemeMode.Dark ? $"-{theme.Mode.ToString().ToLowerInvariant()}" : "";
            return new HtmlString(htmlHelper.Encode(content));
        }

        public static IHtmlContent GetThemeStyleSuffix(this IHtmlHelper htmlHelper)
        {
            var theme = AsyncHelper.RunSync(() => htmlHelper.ViewContext.HttpContext.GetThemeAsync());

            var content = $"-{theme.Style.ToString().ToLowerInvariant()}";
            return new HtmlString(htmlHelper.Encode(content));
        }
    }

    public static class SiteExtensions
    {
        public static IHtmlHelper AddTitle(this IHtmlHelper htmlHelper, string title)
        {
            htmlHelper.AddContent("Title", title);
            return htmlHelper;
        }
        public static string GetTitle(this IHtmlHelper htmlHelper)
        {
            return htmlHelper.GetContent("Title", " • ");
        }


        public static IHtmlHelper AddDescription(this IHtmlHelper htmlHelper, string description)
        {
            htmlHelper.AddContent("Description", description);
            return htmlHelper;
        }
        public static string GetDescription(this IHtmlHelper htmlHelper)
        {
            return htmlHelper.GetContent("Description") as string;
        }


        public static IHtmlHelper AddGraph(this IHtmlHelper htmlHelper, OpenGraph graph)
        {
            htmlHelper.AddContent("Graph", graph);
            return htmlHelper;
        }
        public static OpenGraph GetGraph(this IHtmlHelper htmlHelper)
        {
            return htmlHelper.GetContent("Graph") as OpenGraph;
        }


        public static object GetContent(this IHtmlHelper htmlHelper, string key)
        {
            if (key == null)
                throw new ArgumentNullException(nameof(key));

            key += "Content";

            var contentList = (htmlHelper.ViewData[key] as IEnumerable<object>) ?? Enumerable.Empty<object>();

            return contentList.FirstOrDefault();
        }
        public static string GetContent(this IHtmlHelper htmlHelper, string key, string separator)
        {
            if (key == null)
                throw new ArgumentNullException(nameof(key));

            key += "Content";

            var contentList = ((htmlHelper.ViewData[key] as IEnumerable<object>) ?? Enumerable.Empty<object>()).Select(x => x.ToString());
            contentList = contentList.Where(x => !string.IsNullOrWhiteSpace(x));


            var content = string.Join(separator, contentList.Select(c => c.Trim())).Trim();
            return content;
        }
        public static void AddContent(this IHtmlHelper htmlHelper, string key, object content)
        {
            if (key == null)
                throw new ArgumentNullException(nameof(key));

            key += "Content";

            if (content != null)
            {
                var contentList = (htmlHelper.ViewData[key] as IEnumerable<object>) ?? Enumerable.Empty<object>();
                htmlHelper.ViewData[key] = contentList.Append(content);
            }
        }
    }
}