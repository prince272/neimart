using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.WebUtilities;
using Neimart.Core.Utilities.Extensions;

namespace Neimart.Core.Infrastructure.Web
{
    public static class XQueryHelpers
    {
        public static string RemoveQueryString(string url, string name)
        {
            return RemoveQueryString(url, new[] { name });
        }

        public static string RemoveQueryString(string url, IEnumerable<string> names)
        {
            Uri uri;
            bool urlRelative;

            if (Uri.IsWellFormedUriString(url, UriKind.Relative))
            {
                uri = new Uri(string.Concat("http://www.example.com", url));
                urlRelative = true;
            }
            else
            {
                uri = new Uri(url);
                urlRelative = false;
            }

            // this gets all the query string key value pairs as a collection
            var query = QueryHelpers.ParseQuery(uri.Query).ToDictionary(x => x.Key, x => x.Value.ToString() as string);

            // this removes the key if exists
            foreach (var name in names) query.Remove(name);

            // this gets the url without the query string.
            string urlWithoutQueryString = urlRelative ? uri.AbsolutePath : uri.GetLeftPart(UriPartial.Path);
            string queryString = new QueryBuilder(query).ToQueryString().ToUriComponent();

            return $"{urlWithoutQueryString}{queryString}";
        }
    }
}