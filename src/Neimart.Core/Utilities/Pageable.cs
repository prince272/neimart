using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace Neimart.Core.Utilities
{
    public class Pageable<T> : IPageable<T>
    {
        private readonly IEnumerable<T> _items;

        public Pageable(IQueryable<T> source, int page, int pageSize, bool fallback)
        {
            if (page <= 0) page = Math.Max(page, 1);

            if (pageSize <= 0)
                throw new ArgumentException("Page size must be greater than zero.", nameof(pageSize));

            int totalItems = source.Count();

            // Ensure the total pages does not fall below 1.
            int totalPages = Math.Max((int)Math.Ceiling((double)totalItems / pageSize), 1);

            // Ensure the skip items does not exceed the total items.
            int skipItems = Math.Min((page - 1) * pageSize, totalItems);

            if (fallback)
            {
                // Ensure the remaining items does not fall below 0.
                int remainingItems = Math.Max(pageSize - (totalItems - skipItems), 0);

                if (skipItems > remainingItems)
                    skipItems -= remainingItems;
            }

            _items = source.Skip(skipItems).Take(pageSize).ToList();

            Page = page;
            PageSize = pageSize;
            TotalPages = totalPages;
            TotalItems = totalItems;
        }

        public Pageable(IQueryable<T> source)
            : this(source, 1, int.MaxValue, false)
        {
        }

        public int Page { get; }

        public int PageSize { get; }

        public int TotalPages { get; }

        public int TotalItems { get; }

        public int PageFrom => ((Page * PageSize) - PageSize) + 1;

        public int PageTo => PageSize * Page;

        public IEnumerator<T> GetEnumerator()
        {
            foreach (var item in _items)
            {
                yield return item;
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }

    public interface IPageable<T> : IEnumerable<T>
    {
        int Page { get; }

        int PageSize { get; }

        int PageFrom { get; }

        int PageTo { get; }

        int TotalPages { get; }

        int TotalItems { get; }
    }

    public static class PageableExtensions
    {
        public static IPageable<T> ToPageable<T>(this IQueryable<T> source, int page, int pageSize, bool fallback = false)
        {
            return new Pageable<T>(source, page, pageSize, fallback);
        }

        public static Task<IPageable<T>> ToPageableAsync<T>(this IQueryable<T> source, int page, int pageSize, bool fallback = false)
        {
            return Task.FromResult(ToPageable(source, page, pageSize, fallback));
        }

        public static async Task<IPageable<T>> SuggestAsync<T>(this IQueryable<T> source, int position, int length)
        {

            int totalItems = await source.CountAsync();
            int totalPages = Math.Max(1, (totalItems + length - 1) / length);

            // Note: Not all pages will be shown when the total number of pages exceeds the day of year.
            // We believe that it can never happen.
            position = (position - 1) + DateTimeOffset.UtcNow.DayOfYear;
            position = ((position - 1) % totalPages) + 1;

            var result = await source.ToPageableAsync(position, length, fallback: true);
            return result;
        }

        public static IPageable<T> ToPageable<T>(this IEnumerable<T> source)
        {
            return new Pageable<T>(source.AsQueryable());
        }

        public static Task<IPageable<T>> ToPageableAsync<T>(this IEnumerable<T> source)
        {
            return Task.FromResult(ToPageable(source));
        }
    }
}
