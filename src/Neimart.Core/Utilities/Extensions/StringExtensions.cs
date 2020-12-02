using System;
using System.Collections.Generic;
using System.Text;

namespace Neimart.Core.Utilities.Extensions
{
    public static class StringExtensions
    {
        public static string TrimStart(this string inputText, string value, StringComparison comparisonType = StringComparison.CurrentCulture)
        {
            if (!string.IsNullOrEmpty(value))
            {
                while (!string.IsNullOrEmpty(inputText) && inputText.StartsWith(value, comparisonType))
                {
                    inputText = inputText.Substring(value.Length - 1);
                }
            }

            return inputText;
        }

        public static string TrimEnd(this string inputText, string value, StringComparison comparisonType = StringComparison.CurrentCulture)
        {
            if (!string.IsNullOrEmpty(value))
            {
                while (!string.IsNullOrEmpty(inputText) && inputText.EndsWith(value, comparisonType))
                {
                    inputText = inputText.Substring(0, (inputText.Length - value.Length));
                }
            }

            return inputText;
        }

        public static string Trim(this string inputText, string value, StringComparison comparisonType = StringComparison.CurrentCulture)
        {
            return TrimStart(TrimEnd(inputText, value, comparisonType), value, comparisonType);
        }
    }
}