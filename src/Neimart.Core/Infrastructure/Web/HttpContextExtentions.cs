using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using Humanizer;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;
using Neimart.Core;
using Neimart.Core.Entities;
using Neimart.Core.Services;
using Neimart.Core.Settings;
using Neimart.Core.Utilities.Helpers;

namespace Neimart.Core.Infrastructure.Web
{
    public static class HttpContextExtentions
    {
        public static async Task<User> GetMemberAsync(this HttpContext httpContext)
        {
            if (httpContext == null) throw new ArgumentNullException(nameof(httpContext));

            var userService = httpContext.RequestServices.GetRequiredService<UserService>();

            var currentMemberItemKey = "CurrentMember";
            var currentUser = httpContext.Items[currentMemberItemKey] as User;

            if (currentUser == null)
            {
                if (httpContext.User != null)
                {
                    var currentUserId = long.TryParse(userService.GetUserId(httpContext.User), out long tempStoreId) ? tempStoreId : (long?)null;

                    if (currentUserId != null)
                        currentUser = await userService.FindByIdAsync(currentUserId.Value);
                }
            }

            httpContext.Items[currentMemberItemKey] = currentUser;

            return currentUser;
        }

        public static ValueTask<string> GetIpAddressAsync(this HttpContext httpContext)
        {
            if (httpContext == null) throw new ArgumentNullException(nameof(httpContext));

            var result = string.Empty;
            try
            {
                //first try to get IP address from the forwarded header
                if (httpContext.Request.Headers != null)
                {
                    //the X-Forwarded-For (XFF) HTTP header field is a de facto standard for identifying the originating IP address of a client
                    //connecting to a web server through an HTTP proxy or load balancer
                    var forwardedHttpHeaderKey = "X-FORWARDED-FOR";

                    var forwardedHeader = httpContext.Request.Headers[forwardedHttpHeaderKey];
                    if (!StringValues.IsNullOrEmpty(forwardedHeader))
                        result = forwardedHeader.FirstOrDefault();
                }

                //if this header not exists try get connection remote IP address
                if (string.IsNullOrEmpty(result) && httpContext.Connection.RemoteIpAddress != null)
                    result = httpContext.Connection.RemoteIpAddress.ToString();
            }
            catch { return new ValueTask<string>(string.Empty); }

            //some of the validation
            if (result != null && result.Equals("::1", StringComparison.OrdinalIgnoreCase))
                result = "127.0.0.1";

            //"TryParse" doesn't support IPv4 with port number
            if (IPAddress.TryParse(result ?? string.Empty, out IPAddress ip))
                //IP address is valid 
                result = ip.ToString();
            else if (!string.IsNullOrEmpty(result))
                //remove port
                result = result.Split(':').FirstOrDefault();

            return new ValueTask<string>(result);
        }

        public static async Task<User> GetSellerAsync(this HttpContext httpContext)
        {
            if (httpContext == null) throw new ArgumentNullException(nameof(httpContext));

            var userService = httpContext.RequestServices.GetRequiredService<UserService>();
            string currentSellerItemKey = "CurrentSeller";
            var currentSeller = httpContext.Items[currentSellerItemKey] as User;
            var currentMember = await httpContext.GetMemberAsync();

            if (currentSeller == null)
            {
                // Find the seller by the request path.
                string controller = httpContext.Request.RouteValues["controller"]?.ToString().ToLowerInvariant();
                string storeSlug = httpContext.Request.Path.ToUriComponent().Split(new char[] { '/' }, StringSplitOptions.RemoveEmptyEntries).FirstOrDefault();

                if (string.Equals(controller, "Store", StringComparison.InvariantCultureIgnoreCase) && !string.IsNullOrWhiteSpace(storeSlug))
                {
                    // Pass the member if their store slug are the same.
                    if (string.Equals(currentMember?.StoreSlug, storeSlug, StringComparison.InvariantCultureIgnoreCase))
                        currentSeller = currentMember;
                    else
                        currentSeller = await userService.FindByStoreSlugAsync(storeSlug);
                }
            }

            if (currentSeller == null)
            {
                // Find the seller by the resolved path.
                string storeSlug = httpContext.GetResolvedPath().Split(new char[] { '/' }, StringSplitOptions.RemoveEmptyEntries).FirstOrDefault();

                if (!string.IsNullOrWhiteSpace(storeSlug))
                {
                    // Pass the member if their store slug are the same.
                    if (string.Equals(currentMember?.StoreSlug, storeSlug, StringComparison.InvariantCultureIgnoreCase))
                        currentSeller = currentMember;
                    else
                        currentSeller = await userService.FindByStoreSlugAsync(storeSlug);
                }
            }

            httpContext.Items[currentSellerItemKey] = currentSeller;

            return currentSeller;
        }

        public static async Task<Theme> GetThemeAsync(this HttpContext httpContext)
        {
            var currentThemeItemKey = "CurrentTheme";
            var currentTheme = httpContext.Items[currentThemeItemKey] as Theme;

            if (currentTheme == null)
            {
                var linkGenerator = httpContext.RequestServices.GetRequiredService<LinkGenerator>();
                var resolvedPath = httpContext.GetResolvedPath();

                string portalPath = linkGenerator.GetPathByAction(action: "Index", controller: "Home", values: new { area = "Portal" });
                string currentPath = httpContext.Request.Path;

                var accountPaths = new string[] {
                    linkGenerator.GetPathByAction(action: "EditProfile", controller: "Account", values: new { area = string.Empty }),
                    linkGenerator.GetPathByAction(action: "ManageWallet", controller: "Account", values: new { area = string.Empty })
                }.AsEnumerable();

                if (currentTheme == null)
                {
                    var member = await httpContext.GetMemberAsync();

                    if (member != null)
                    {
                        if (resolvedPath.StartsWith(portalPath))
                        {
                            currentTheme = new Theme
                            {
                                Mode = member.StoreThemeMode,
                                Style = member.StoreThemeStyle,
                                HomeUrl = portalPath,
                                Area = ThemeArea.Portal
                            };

                            if (accountPaths.Any(x => currentPath.StartsWith(x, StringComparison.InvariantCultureIgnoreCase)))
                            {
                                currentTheme.Layout = "~/Areas/Portal/Views/Account/_Layout.cshtml";
                            }
                            else
                            {
                                currentTheme.Layout = "~/Areas/Portal/Views/Shared/_Layout.cshtml";
                            }
                        }
                    }
                }

                if (currentTheme == null)
                {
                    var seller = await httpContext.GetSellerAsync();

                    if (seller != null)
                    {
                        string storePath = linkGenerator.GetPathByAction(action: "Index", controller: "Store", values: new { area = string.Empty, storeSlug = seller.StoreSlug });

                        if (resolvedPath.StartsWith(storePath))
                        {
                            currentTheme = new Theme
                            {
                                Mode = seller.StoreThemeMode,
                                Style = seller.StoreThemeStyle,
                                HomeUrl = storePath,
                                Area = ThemeArea.Store
                            };

                            if (accountPaths.Any(x => currentPath.StartsWith(x, StringComparison.InvariantCultureIgnoreCase)))
                            {
                                currentTheme.Layout = "~/Views/Store/_AccountLayout.cshtml";
                            }
                            else
                            {
                                currentTheme.Layout = "~/Views/Store/_Layout.cshtml";
                            }
                        }
                    }
                }

                if (currentTheme == null)
                {
                    var appSettings = httpContext.RequestServices.GetRequiredService<IOptions<AppSettings>>().Value;
                    var companyPath = linkGenerator.GetPathByAction(httpContext, action: "Index", controller: "Company", new { area = string.Empty });

                    currentTheme = new Theme
                    {
                        Mode = appSettings.ThemeMode,
                        Style = appSettings.ThemeStyle,
                        HomeUrl = companyPath,
                        Area = ThemeArea.Company,
                        Layout = "~/Views/Company/_Layout.cshtml"
                    };
                }
            }

            httpContext.Items[currentThemeItemKey] = currentTheme;

            return currentTheme;
        }

        public static string GetResolvedPath(this HttpContext httpContext)
        {
            var exceptionFeature = httpContext.Features.Get<IExceptionHandlerPathFeature>();
            var statusCodeFeature = httpContext.Features.Get<IStatusCodeReExecuteFeature>();

            var resolvedPath = (exceptionFeature?.Path ?? string.Concat(statusCodeFeature?.OriginalPath, statusCodeFeature?.OriginalQueryString));
            if (string.IsNullOrWhiteSpace(resolvedPath)) resolvedPath = httpContext.Request.Query["returnUrl"];
            if (string.IsNullOrWhiteSpace(resolvedPath)) resolvedPath = httpContext.Request.Path;

            var returnUrl = QueryHelpers.ParseQuery(new Uri(string.Concat("http://www.example.com", resolvedPath)).Query).GetValueOrDefault("returnUrl");

            if (!string.IsNullOrWhiteSpace(returnUrl))
                resolvedPath = returnUrl;

            return resolvedPath; // resolved
        }

        //public static bool TryGetRouteValuesFromPath(this HttpContext httpContext, string endpointName, string path, out RouteValueDictionary routeValues)
        //{
        //    routeValues = null;

        //    try
        //    {
        //        var linkParser = httpContext.RequestServices.GetRequiredService<LinkParser>();
        //        routeValues = linkParser.ParsePathByEndpointName(endpointName, path);
        //        return routeValues != null;
        //    }
        //    catch (ArgumentException)
        //    {

        //    }

        //    return false;
        //}
    }
}