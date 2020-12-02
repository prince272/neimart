using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Humanizer.Localisation;
using Microsoft.AspNetCore.Mvc;

namespace Neimart.Core.Utilities.Helpers
{
    public static class EnumHelper
    {
        public static IEnumerable<TEnum> GetEnumValues<TEnum>()
            where TEnum : struct
        {
            var enumValues = Enum.GetValues(typeof(TEnum)).Cast<TEnum>().ToList();
            return enumValues;
        }
    }
}