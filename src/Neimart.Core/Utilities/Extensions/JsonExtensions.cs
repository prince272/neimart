using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;

namespace Neimart.Core.Utilities.Extensions
{
    public static class JsonExtensions
    {
        /// <summary>
        /// Converts given object to JSON string.
        /// </summary>
        /// <returns></returns>
        public static string ToJsonString(this object obj, bool camelCase = true, bool indented = false)
        {
            var options = new JsonSerializerSettings();

            if (indented)
            {
                options.Formatting = Formatting.Indented;
            }

            if (camelCase)
            {
                options.ContractResolver = new CamelCasePropertyNamesContractResolver();
            }

            options.Converters.Add(new StringEnumConverter(new DefaultNamingStrategy()));
            options.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;

            return JsonConvert.SerializeObject(obj, options);
        }

        /// <summary>
        /// Converts given JSON string to object.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="str"></param>
        /// <param name="camelCase"></param>
        /// <param name="indented"></param>
        /// <returns></returns>
        public static T ToJsonObject<T>(this string str, bool camelCase = true, bool indented = false)
        {
            var options = new JsonSerializerSettings();

            if (indented)
            {
                options.Formatting = Formatting.Indented;
            }

            if (camelCase)
            {
                options.ContractResolver = new CamelCasePropertyNamesContractResolver();
            }

            options.Converters.Add(new StringEnumConverter(new DefaultNamingStrategy()));

            return JsonConvert.DeserializeObject<T>(str, options);
        }
    }
}
