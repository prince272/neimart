using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Neimart.Core;
using Neimart.Core.Entities;
using Neimart.Core.Services;
using Neimart.Core.Utilities.Helpers;

namespace Neimart.Core.Infrastructure.Web
{
    public class StoreSlugRouteConstraint : IRouteConstraint
    {
        private readonly object lockObject = new object();
        private readonly IServiceProvider _serviceProvider;

        public StoreSlugRouteConstraint(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public bool Match(
            HttpContext httpContext,
            IRouter route,
            string routeKey,
            RouteValueDictionary routeValues,
            RouteDirection routeDirection)
        {
            if (routeValues.TryGetValue(routeKey, out var value) && value != null)
            {
                if (value is string storeSlug && !string.IsNullOrWhiteSpace(storeSlug))
                {
                    // TODO: This is a little bug fix to ensure we get the HttpContext.
                    // source: https://github.com/dotnet/aspnetcore/issues/20773

                    httpContext ??= _serviceProvider.GetRequiredService<IHttpContextAccessor>().HttpContext;
                    var userService = httpContext.RequestServices.GetRequiredService<UserService>();
                    var matchStoreSlugItemKey = $"MatchStoreSlug:{storeSlug}";

                    var seller = httpContext.Items[matchStoreSlugItemKey];

                    if (seller == null)
                    {
                        seller = AsyncHelper.RunSync(() => userService.FindByStoreSlugAsync(storeSlug));
                        httpContext.Items[matchStoreSlugItemKey] = seller;
                    }

                    return seller != null;
                }
            }

            return false;
        }
    }
}