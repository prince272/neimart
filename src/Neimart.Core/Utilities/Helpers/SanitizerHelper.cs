using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using Ganss.XSS;

namespace Neimart.Core.Utilities.Helpers
{
    public static class SanitizerHelper
    {
        public static string ConvertHtmlToText(string html)
        {
            if (string.IsNullOrWhiteSpace(html))
                return html?.Trim();

            const string tagWhiteSpace = @"(>|$)(\W|\n|\r)+<";//matches one or more (white space or line breaks) between '>' and '<'
            const string stripFormatting = @"<[^>]*(>|$)";//match any character between '<' and '>', even when end tag is missing
            const string lineBreak = @"<(br|BR)\s{0,1}\/{0,1}>";//matches: <br>,<br/>,<br />,<BR>,<BR/>,<BR />
            var lineBreakRegex = new Regex(lineBreak, RegexOptions.Multiline | RegexOptions.Compiled);
            var stripFormattingRegex = new Regex(stripFormatting, RegexOptions.Multiline | RegexOptions.Compiled);
            var tagWhiteSpaceRegex = new Regex(tagWhiteSpace, RegexOptions.Multiline | RegexOptions.Compiled);

            var text = html;
            //Decode html specific characters
            text = System.Net.WebUtility.HtmlDecode(text);
            //Remove tag whitespace/line breaks
            text = tagWhiteSpaceRegex.Replace(text, "><");
            //Replace <br /> with line breaks
            text = lineBreakRegex.Replace(text, Environment.NewLine);
            //Strip formatting
            text = stripFormattingRegex.Replace(text, string.Empty).Trim();

            return text;
        }

        public static string SanitizeHtml(string html)
        {
            if (string.IsNullOrWhiteSpace(html))
                return html?.Trim();

            var sanitizer = new HtmlSanitizer();

            sanitizer.AllowedAttributes.Add("class");
            sanitizer.AllowedSchemes.Add("data");

            var sanitized = sanitizer.Sanitize(html);

            return sanitized;
        }

        public static string RemoveDiacritics(string text)
        {
            var normalizedString = text.Normalize(NormalizationForm.FormD);
            var stringBuilder = new StringBuilder();

            foreach (var c in normalizedString)
            {
                var unicodeCategory = CharUnicodeInfo.GetUnicodeCategory(c);
                if (unicodeCategory != UnicodeCategory.NonSpacingMark)
                {
                    stringBuilder.Append(c);
                }
            }

            return stringBuilder.ToString().Normalize(NormalizationForm.FormC);
        }

        public static string RemoveAccent(string text)
        {
            byte[] bytes = Encoding.GetEncoding("Cyrillic").GetBytes(text);
            return Encoding.ASCII.GetString(bytes);
        }

        public static string ExtractPhoneNumber(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return text?.Trim();

            return Regex.Replace(text, "[^0-9+]+", string.Empty, RegexOptions.Compiled);
        }

        public static string ExtractNumeric(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return text?.Trim();

            return Regex.Replace(text, "[^0-9]+", string.Empty, RegexOptions.Compiled);
        }

        public static string ExtractAlphaNumeric(string text)
        {
            return Regex.Replace(text, "[^A-Za-z0-9]+", string.Empty, RegexOptions.Compiled);
        }

        public static string ExtractAlpha(string text)
        {
            return Regex.Replace(text, "[^A-Za-z]+", string.Empty, RegexOptions.Compiled);
        }

        // URL Slugify algorithm in C#?
        // source: https://stackoverflow.com/questions/2920744/url-slugify-algorithm-in-c/2921135#2921135
        public static string SanitizeText(string text, bool replaceSymbolsWithHyphen = false, bool useLowercase = false, bool useUppercase = false)
        {
            if (string.IsNullOrWhiteSpace(text))
                return text?.Trim();

            // remove all diacritics.
            text = RemoveDiacritics(text);

            if (useLowercase) text = text.ToLowerInvariant();

            if (useUppercase) text = text.ToUpperInvariant();

            // Remove everything that's not a letter, number, hyphen, dot, whitespace or underscore.
            text = Regex.Replace(text, @"[^a-zA-Z0-9\-\.\s_]", string.Empty, RegexOptions.Compiled);

            if (replaceSymbolsWithHyphen)
            {
                // replace symbols with a hyphen.
                text = Regex.Replace(text, @"[\-\.\s_]", "-", RegexOptions.Compiled);

                // replace double occurrences of hyphen.
                text = Regex.Replace(text, @"(-){2,}", "$1", RegexOptions.Compiled).Trim('-');
            }

            return text;
        }

        public static string GenerateSlug(string name, Func<string, bool> exists = null)
        {
            var slug = SanitizeText(name, replaceSymbolsWithHyphen: true, useLowercase: true);
            int count = 1;

            while (string.IsNullOrWhiteSpace(slug) || (exists?.Invoke(slug) ?? false))
            {
                count += 1;
                slug = SanitizeText($"{name} {count}", replaceSymbolsWithHyphen: true, useLowercase: true);
            }

            return slug;
        }

        public static string AppendUrlScheme(this string url)
        {
            if (string.IsNullOrWhiteSpace(url))
                return url?.Trim();

            // Prepending “http://” to a URL that doesn't already contain “http://”
            // source: https://stackoverflow.com/questions/3543187/prepending-http-to-a-url-that-doesnt-already-contain-http

            var prefix = "http://";
            var pattern = @"^https?:\/\/";

            if (!Regex.IsMatch(url, pattern, RegexOptions.IgnoreCase | RegexOptions.Compiled))
            {
                url = $"{prefix}{url}";
            }

            return url;
        }

        public static string AppendUrlPath(this string url)
        {
            if (string.IsNullOrWhiteSpace(url))
                return url?.Trim();

            url = $"/{url.TrimStart('/')}";

            return url;
        }
    }
}