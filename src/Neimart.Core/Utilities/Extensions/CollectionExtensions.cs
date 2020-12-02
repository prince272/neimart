using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Neimart.Core.Utilities.Helpers;

namespace Neimart.Core.Utilities.Extensions
{
    public static class CollectionExtensions
    {
        public static void AddRange<T>(this ICollection<T> source, IEnumerable<T> collection)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            if (source == null) throw new ArgumentNullException(nameof(collection));

            foreach (var item in collection)
            {
                source.Add(item);
            }
        }

        public static void RemoveRange<T>(this ICollection<T> source, IEnumerable<T> collection)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            if (source == null) throw new ArgumentNullException(nameof(collection));

            foreach (var item in collection)
            {
                source.Remove(item);
            }
        }

        public static IEnumerable<T> Randomize<T>(this IEnumerable<T> source)
        {
            var rnd = new Random();
            return source.OrderBy<T, int>((item) => rnd.Next());
        }

        public static List<List<T>> Batch<T>(this ICollection<T> source, int batchSize)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));

            var allBatches = new List<List<T>>();

            if (source.Count > 0)
            {
                var totalNumberOfBatches = (int)Math.Ceiling(source.Count / (float)batchSize);

                for (var i = 0; i < totalNumberOfBatches; i++)
                {
                    var batch = source.Skip(i * batchSize).Take(batchSize).ToList();
                    allBatches.Add(batch);
                }
            }

            return allBatches;
        }

        public static TValue ValueOrDefault<TKey, TValue>(this IDictionary<TKey, object> dictionary, TKey key)
        {
            return dictionary.TryGetValue(key, out var obj) ? TypeHelper.ConvertToObject<TValue>(obj) : default;
        }

        public static TValue ValueOrDefault<TValue>(this IDictionary<string, object> dictionary, string key)
        {
            return dictionary.ValueOrDefault<string, TValue>(key);
        }

        // TODO: Testing required. IndexOf, Next, Previous, NextOrDefault, PreviousOrDefault methods.
        public static int IndexOf<T>(this IEnumerable<T> source, T value)
        {
            int index = 0;
            var comparer = EqualityComparer<T>.Default; // or pass in as a parameter
            foreach (T item in source)
            {
                if (comparer.Equals(item, value)) return index;
                index++;
            }
            return -1;
        }

        public static T Next<T>(this IEnumerable<T> source, T value)
        {
            int index = source.IndexOf(value) + 1;
            return source.ElementAt(index);
        }

        public static T Previous<T>(this IEnumerable<T> source, T value)
        {
            int index = source.IndexOf(value) - 1;
            return source.ElementAt(index);
        }

        public static T NextOrDefault<T>(this IEnumerable<T> source, T value, T defaultValue = default)
        {
            int length = source.Count();
            int index = source.IndexOf(value) + 1;
            return (index >= 0 && index < length) ? source.ElementAt(index) : defaultValue;
        }

        public static T PreviousOrDefault<T>(this IEnumerable<T> source, T value, T defaultValue = default)
        {
            int length = source.Count();
            int index = source.IndexOf(value) - 1;
            return (index >= 0 && index < length) ? source.ElementAt(index) : defaultValue;
        }
    }
}