using System;
using System.Collections.Generic;
using Neimart.Core.Utilities.Helpers;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Neimart.Core.Utilities.Extensions
{
    public interface IExtendable
    {
        /// <summary>
        /// A JSON formatted string to extend the containing object.
        /// JSON data can contain properties with arbitrary values (like primitives or complex objects).
        /// Extension methods are available (<see cref="ExtendableExtensions"/>) to manipulate this data.
        /// General format:
        /// <code>
        /// {
        ///   "Property1" : ...
        ///   "Property2" : ...
        /// }
        /// </code>
        /// </summary>
        string ExtensionData { get; set; }
    }

    public static class ExtendableExtensions
    {
        public static T GetValue<T>(this IExtendable extendable, string name, bool handleType = false)
        {
            return extendable.GetValue<T>(
                name,
                handleType
                    ? new JsonSerializer { TypeNameHandling = TypeNameHandling.All }
                    : JsonSerializer.CreateDefault()
            );
        }

        public static T GetValue<T>(this IExtendable extendable, string name, JsonSerializer jsonSerializer)
        {
            if (extendable == null) throw new ArgumentNullException(nameof(extendable));
            if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Value cannot be null, empty, or consists only of white-space.", nameof(name));

            if (extendable.ExtensionData == null)
            {
                return default(T);
            }

            var json = JObject.Parse(extendable.ExtensionData);

            var prop = json[name];
            if (prop == null)
            {
                return default(T);
            }

            if (TypeHelper.IsPrimitiveExtendedIncludingNullable(typeof(T)))
            {
                return prop.Value<T>();
            }
            else
            {
                return (T)prop.ToObject(typeof(T), jsonSerializer ?? JsonSerializer.CreateDefault());
            }
        }

        public static void SetValue<T>(this IExtendable extendable, string name, T value, bool handleType = false)
        {
            extendable.SetValue(
                name,
                value,
                handleType
                    ? new JsonSerializer { TypeNameHandling = TypeNameHandling.All }
                    : JsonSerializer.CreateDefault()
            );
        }

        public static void SetValue<T>(this IExtendable extendable, string name, T value, JsonSerializer jsonSerializer)
        {
            if (extendable == null) throw new ArgumentNullException(nameof(extendable));
            if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Value cannot be null, empty, or consists only of white-space.", nameof(name));

            if (jsonSerializer == null)
            {
                jsonSerializer = JsonSerializer.CreateDefault();
            }

            if (extendable.ExtensionData == null)
            {
                if (EqualityComparer<T>.Default.Equals(value, default(T)))
                {
                    return;
                }

                extendable.ExtensionData = "{}";
            }

            var json = JObject.Parse(extendable.ExtensionData);

            if (value == null || EqualityComparer<T>.Default.Equals(value, default(T)))
            {
                if (json[name] != null)
                {
                    json.Remove(name);
                }
            }
            else if (TypeHelper.IsPrimitiveExtendedIncludingNullable(value.GetType()))
            {
                json[name] = new JValue(value);
            }
            else
            {
                json[name] = JToken.FromObject(value, jsonSerializer);
            }

            var data = json.ToString(Formatting.None);
            if (data == "{}")
            {
                data = null;
            }

            extendable.ExtensionData = data;
        }

        public static bool RemoveValue(this IExtendable extendable, string name)
        {
            if (extendable == null) throw new ArgumentNullException(nameof(extendable));
            if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Value cannot be null, empty, or consists only of white-space.", nameof(name));

            if (extendable.ExtensionData == null)
            {
                return false;
            }

            var json = JObject.Parse(extendable.ExtensionData);

            var token = json[name];
            if (token == null)
            {
                return false;
            }

            json.Remove(name);

            var data = json.ToString(Formatting.None);
            if (data == "{}")
            {
                data = null;
            }

            extendable.ExtensionData = data;

            return true;
        }
    }
}