using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Newtonsoft.Json;

namespace Neimart.Data.Extensions
{
    public static class JsonConversionExtensions
    {
        public static PropertyBuilder<T> HasJsonConversion<T>(this PropertyBuilder<T> propertyBuilder)
        {
            var settings = new JsonSerializerSettings
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore
            };

            var converter = new ValueConverter<T, string>(
                v => JsonConvert.SerializeObject(v, settings),
                v => JsonConvert.DeserializeObject<T>(v, settings));

            var comparer = new ValueComparer<T>(
                (l, r) => JsonConvert.SerializeObject(l, settings) == JsonConvert.SerializeObject(r, settings),
                v => v == null ? 0 : JsonConvert.SerializeObject(v, settings).GetHashCode(),
                v => JsonConvert.DeserializeObject<T>(JsonConvert.SerializeObject(v, settings)));

            propertyBuilder.HasConversion(converter);
            propertyBuilder.Metadata.SetValueConverter(converter);
            propertyBuilder.Metadata.SetValueComparer(comparer);

            return propertyBuilder;
        }
    }
}