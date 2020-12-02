using System;
using System.Collections.Generic;
using System.Text;

namespace Neimart.Core.Utilities.Extensions
{
    // Convert C# URI/URL to Absolute or Relative
    // source: https://sebnilsson.com/blog/convert-c-uri-url-to-absolute-or-relative/
    public static class UriExtensions
    {
        public static string ToRelative(this Uri uri)
        {
            // TODO: Null-checks

            return uri.IsAbsoluteUri ? uri.PathAndQuery : uri.OriginalString;
        }

        public static string ToAbsolute(this Uri uri, string baseUrl)
        {
            // TODO: Null-checks

            var baseUri = new Uri(baseUrl);

            return uri.ToAbsolute(baseUri);
        }

        public static string ToAbsolute(this Uri uri, Uri baseUri)
        {
            // TODO: Null-checks

            var relative = uri.ToRelative();

            if (Uri.TryCreate(baseUri, relative, out var absolute))
            {
                return absolute.ToString();
            }

            return uri.IsAbsoluteUri ? uri.ToString() : null;
        }
    }
}
