using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Humanizer;
using Neimart.Core.Entities;
using Neimart.Core.Utilities.Helpers;

namespace Neimart.Core.Settings
{
    public class AppSettings
    {
        public CompanyInfo Company { get; set; }

        public IEnumerable<decimal> PlanRates { get; set; }

        public int PlanTrialDays { get; set; }

        public int CartMaxCount { get; set; }

        public ThemeMode ThemeMode { get; set; }

        public ThemeStyle ThemeStyle { get; set; }

        public int PageDefaultSize { get; set; }

        public int QuantityMaxValue { get; set; }

        public string PercentSymbol { get; set; }

        public string CurrencySymbol { get; set; }

        public string CurrencyCode { get; set; }

        public decimal CurrencyMinValue { get; set; }

        public decimal CurrencyMaxValue { get; set; }

        public string CountryCode { get; set; }

        public long ImageFileMaxSize { get; set; }

        public long DocumentFileMaxSize { get; set; }

        public long AudioFileMaxSize { get; set; }

        public long VideoFileMaxSize { get; set; }

        public string[] ImageFileExtensions { get; set; }

        public string[] DocumentFileExtensions { get; set; }

        public string[] AudioFileExtensions { get; set; }

        public string[] VideoFileExtensions { get; set; }

        public string[] AnyFileExtensions => new string[][] {
            ImageFileExtensions,
            DocumentFileExtensions,
            AudioFileExtensions,
            VideoFileExtensions
        }.SelectMany(x => x).ToArray();

        public long AnyFileMaxSize => new long[] {
            ImageFileMaxSize,
            DocumentFileMaxSize,
            AudioFileMaxSize,
            VideoFileMaxSize
        }.Max();

        public List<string[]> PlanFeatures { get; set; }

        /// <summary>
        /// Get and set the payment processing fee in percentage to use during transaction.
        /// </summary>
        public decimal PaymentRate { get; set; }

        public bool IsAllowedFileExtension(string fileExtension)
        {
            return IsAllowedFileExtension(fileExtension, AnyFileExtensions);
        }

        public bool IsAllowedFileExtension(string fileExtension, string[] fileExtensions)
        {
            if (fileExtension == null)
                throw new ArgumentNullException(nameof(fileExtension));

            return fileExtensions.Contains(fileExtension);
        }

        public bool IsAllowedFileSize(long fileSize)
        {
            return IsAllowedFileSize(fileSize, AnyFileMaxSize);
        }

        public bool IsAllowedFileSize(long fileSize, long fileMaxSize)
        {
            return fileSize <= fileMaxSize;
        }

        public string FormatCurrency(decimal value)
        {
            return $"{CurrencySymbol} {value:G29}";
        }

        public string FormatCurrency(string value)
        {
            return $"{CurrencySymbol} {value:G29}".Trim();
        }

        public string FormatPercent(decimal value)
        {
            return $"{value:G29}{PercentSymbol}";
        }

        public string FormatDateTime(DateTimeOffset value)
        {
            return $"{FormatDate(value)} at {FormatTime(value)}"; // "3/9/2008 4:05 PM"
        }

        public string FormatDate(DateTimeOffset value)
        {
            return string.Format("{0: d MMM yyyy}", value); // "3/9/2008"
        }

        public string FormatTime(DateTimeOffset value)
        {
            return string.Format("{0:t}", value); // "4:05 PM"
        }

        public string ParseGreeting(DateTimeOffset value)
        {
            if (value.Hour < 12)
            {
                return "Good Morning";
            }
            else if (value.Hour < 17)
            {
                return "Good Afternoon";
            }
            else
            {
                return "Good Evening";
            }
        }

        public string FormatPhoneNumber(string value)
        {
            if (!string.IsNullOrWhiteSpace(value))
            {
                try
                {
                    var phoneUtil = PhoneNumbers.PhoneNumberUtil.GetInstance();
                    var phoneNumberObject = phoneUtil.Parse(value, null);
                    var phoneNumber = phoneUtil.Format(phoneNumberObject, PhoneNumbers.PhoneNumberFormat.INTERNATIONAL);
                    return phoneNumber;
                }
                catch (Exception) { }
            }

            return value;
        }

        // String format numbers to millions, thousands with rounding
        // source: https://stackoverflow.com/questions/30180672/string-format-numbers-to-millions-thousands-with-rounding?lq=1
        public string FormatNumber(long value)
        {
            // Ensure number has max 3 significant digits (no rounding up can happen)
            long i = (long)Math.Pow(10, (int)Math.Max(0, Math.Log10(value) - 2));
            value = value / i * i;

            if (value >= 1000000000)
                return (value / 1000000000D).ToString("0.##") + "B";
            if (value >= 1000000)
                return (value / 1000000D).ToString("0.##") + "M";
            if (value >= 1000)
                return (value / 1000D).ToString("0.##") + "K";

            return value.ToString("#,0");
        }

        public string GenerateCode(string prefixName, string suffixName)
        {
            var prefixCode = string.Join(string.Empty, SanitizerHelper.ExtractAlpha(prefixName.ToUpperInvariant()).Take(3));
            var suffixCode = string.Join(string.Empty, SanitizerHelper.ExtractAlpha(suffixName.ToUpperInvariant()).Take(3));
            var code = string.Format("{0}-{1}-{2}", prefixCode, ComputeHelper.GenerateRandomString(4, ComputeHelper.NaturalNumericChars), suffixCode).Trim('-');
            return code;
        }
    }

    public class CompanyInfo
    {
        public string Name { get; set; }

        public string Description { get; set; }

        public string Keywords { get; set; }

        public string AdminEmail { get; set; }

        public string AdminPassword { get; set; }

        public string NotificationEmail { get; set; }

        public string NotificationPassword { get; set; }

        public string InfoEmail { get; set; }

        public string InfoPassword { get; set; }

        public string PhoneNumber { get; set; }

        public string PhoneNumber2 { get; set; }

        public string Address { get; set; }

        public string MapLink { get; set; }

        public string FacebookLink { get; set; }

        public string TwitterLink { get; set; }

        public string YoutubeLink { get; set; }

        public string InstagramLink { get; set; }

        public string LinkedInLink { get; set; }

        public string PinterestLink { get; set; }

        public DateTimeOffset EstablishedOn { get; set; }
    }
}