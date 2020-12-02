using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using Neimart.Core;
using Neimart.Core.Infrastructure.Imaging;
using Neimart.Core.Utilities.Helpers;

namespace Neimart.Core.Infrastructure.Web
{
    public static class UrlHelperExtensions
    {
        // Getting absolute URLs using ASP.NET Core
        // source: https://stackoverflow.com/questions/30755827/getting-absolute-urls-using-asp-net-core
        public static string ContentLink(
             this IUrlHelper url,
             string contentPath, bool includeScheme = true)
        {
            var request = url.ActionContext.HttpContext.Request;

            var contentLink = string.Concat(includeScheme ? request.Scheme + "://" : string.Empty,
                        request.Host.ToUriComponent(), url.Content(contentPath));

            return contentLink;
        }
    }
}