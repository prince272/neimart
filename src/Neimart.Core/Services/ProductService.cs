using System;
using System.Collections.Generic;
using System.Diagnostics.SymbolStore;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Neimart.Core.Entities;
using Neimart.Core.Events;
using Neimart.Core.Filters;
using Neimart.Core.Utilities.Helpers;

namespace Neimart.Core.Services
{
    public class ProductService : EntityService<Product, ProductFilter>
    {
        private readonly CategoryService _categoryService;

        public ProductService(IServiceProvider services) : base(services)
        {
            _categoryService = services.GetRequiredService<CategoryService>();
        }

        public override async Task CreateAsync(Product product)
        {
            if (product == null)
                throw new ArgumentNullException(nameof(product));

            product.CreatedOn = DateTimeOffset.UtcNow;
            product.PublishedOn = product.Published ? DateTimeOffset.UtcNow : default;

            await GenerateSlugAsync(product);
            await base.CreateAsync(product);
        }

        public override async Task UpdateAsync(Product product)
        {
            if (product == null)
                throw new ArgumentNullException(nameof(product));

            product.Updated = true;
            product.UpdatedOn = DateTimeOffset.UtcNow;
            product.PublishedOn = product.Published && product.PublishedOn == default ? DateTimeOffset.UtcNow : product.Published ? product.PublishedOn : default;

            await GenerateSlugAsync(product);
            await base.UpdateAsync(product);
        }

        public override Task DeleteAsync(Product product)
        {
            if (product == null)
                throw new ArgumentNullException(nameof(product));

            product.Deleted = true;
            return UpdateAsync(product);
        }

        public override async Task ClearCacheAsync()
        {
            await _cacheManager.RemoveByPrefixAsync(EntityHelper.GetCachePrefix<Cart>());
            await _cacheManager.RemoveByPrefixAsync(EntityHelper.GetCachePrefix<Review>());
            await base.ClearCacheAsync();
        }

        public override IQueryable<Product> GetQuery(ProductFilter filter = null)
        {

            var query = _unitOfWork.Query<Product>();

            query = query.Include(x => x.Images)
                         .Include(x => x.Document)
                         .Include(x => x.Tags);


            query = query.OrderByDescending(x => !x.Updated ? x.CreatedOn : x.UpdatedOn);

            if (filter != null)
            {
                if (filter.SellerId != null)
                {
                    query = query.Where(x => x.SellerId == filter.SellerId);
                }

                if (filter.ProductId != null)
                {
                    query = query.Where(x => x.Id == filter.ProductId);
                }

                if (filter.Slug != null)
                {
                    query = query.Where(x => !string.IsNullOrWhiteSpace(x.Slug) && x.Slug == filter.Slug);
                }

                if (filter.Published != null)
                {
                    query = query.Where(x => x.Published == filter.Published);
                }

                if (filter.Stock != null)
                {
                    query = query.Where(x => x.Stock == filter.Stock);
                }

                if (filter.DocumentRequired != null)
                {
                    if (filter.DocumentRequired.Value) query = query.Where(x => x.Document != null);
                    else query = query.Where(x => x.Document == null);
                }

                if (filter.MinPrice != null)
                {
                    query = query.Where(x => x.Price >= filter.MinPrice);
                }

                if (filter.MaxPrice != null)
                {
                    query = query.Where(x => x.Price <= filter.MaxPrice);
                }

                if (filter.Discount != null)
                {
                    query = query.Where(x =>
                    (x.OldPrice > 0 && x.OldPrice > x.Price) &&
                    ((100 - Math.Ceiling((x.Price / x.OldPrice) * 100) >= filter.Discount)));
                }

                if (filter.Rating != null)
                {
                    query = query.Where(x => x.Reviews.OrderByDescending(x => x.Rating).Any(x => x.Rating >= filter.Rating));
                }

                if (!string.IsNullOrWhiteSpace(filter.Search))
                {
                    var predicates = new List<Expression<Func<Product, bool>>>();

                    predicates.Add(x => EF.Functions.Like(x.Name, $"%{filter.Search}%") ||
                                          EF.Functions.Like(x.Slug, $"%{filter.Search}%") ||
                                          x.Tags.Any(tag => EF.Functions.Like(tag.Name, $"%{filter.Search}%") ||
                                          x.Tags.Any(tag => EF.Functions.Like(tag.Slug, $"%{filter.Search}%"))));

                    // Search for category tags for possible match.
                    var slugs = _categoryService.GetQuery(filter).SelectMany(x => x.Tags).Select(x => x.Slug).ToList();

                    if (slugs.Any())
                    {
                        predicates.AddRange(slugs.Select(slug => (Expression<Func<Product, bool>>)(x => x.Tags.Any(tag => EF.Functions.Like(tag.Slug, $"%{slug}%")))).ToArray());
                    }

                    query = query.WhereAny(predicates.ToArray());
                }

                if (filter.Sort != null)
                {
                    switch (filter.Sort)
                    {
                        case ProductSort.LowestPrice: query = query.OrderBy(x => x.Price); break;
                        case ProductSort.HighestPrice: query = query.OrderByDescending(x => x.Price); break;

                        case ProductSort.Oldest: query = query.OrderBy(x => x.CreatedOn); break;
                        case ProductSort.Newest: query = query.OrderByDescending(x => x.CreatedOn); break;

                        case ProductSort.Popular: break;

                        case ProductSort.Updated: query = query.OrderByDescending(x => x.UpdatedOn); break;
                    }
                }
            }

            return query;
        }

        protected Task GenerateSlugAsync(Product product)
        {
            product.Slug = SanitizerHelper.GenerateSlug(product.Name,
                productSlug => GetQuery().Any(x => x.SellerId == product.SellerId && x.Id != product.Id && x.Slug == productSlug));

            return Task.CompletedTask;
        }

        public async Task<AmountEvaluation> EvaluateAsync(ProductFilter filter)
        {
            if (filter == null)
                throw new ArgumentNullException(nameof(filter));

            async Task<AmountEvaluation> acquire()
            {
                var query = GetQuery(filter);

                return InternalEvaluate(amount: await query.Select(x => x.Price).SumAsync(),
                                        quantity: await query.CountAsync(),
                                        count: await query.CountAsync());
            }

            string key = await _cacheManager.ComposeKeyAsync($"{EntityHelper.GetCachePrefix<Product>()}.{nameof(EvaluateAsync)}", filter);
            var result = await _cacheManager.GetAsync(key, acquire);
            return result;
        }
    }

    // Query : does EF.Functions.Like support with array words ? #10834
    // source: https://github.com/dotnet/efcore/issues/10834
    public static class DbExtensions
    {
        public static IQueryable<T> WhereAny<T>(this IQueryable<T> queryable, params Expression<Func<T, bool>>[] predicates)
        {
            var parameter = Expression.Parameter(typeof(T));
            return queryable.Where(Expression.Lambda<Func<T, bool>>(predicates.Aggregate<Expression<Func<T, bool>>, Expression>(null,
                                                                                                                                (current, predicate) =>
                                                                                                                                {
                                                                                                                                    var visitor = new ParameterSubstitutionVisitor(predicate.Parameters[0], parameter);
                                                                                                                                    return current != null ? Expression.OrElse(current, visitor.Visit(predicate.Body)) : visitor.Visit(predicate.Body);
                                                                                                                                }),
                                                                    parameter));
        }

        private class ParameterSubstitutionVisitor : ExpressionVisitor
        {
            private readonly ParameterExpression _destination;
            private readonly ParameterExpression _source;

            public ParameterSubstitutionVisitor(ParameterExpression source, ParameterExpression destination)
            {
                _source = source;
                _destination = destination;
            }

            protected override Expression VisitParameter(ParameterExpression node)
            {
                return ReferenceEquals(node, _source) ? _destination : base.VisitParameter(node);
            }
        }

    }
}