using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Neimart.Core.Entities;
using Neimart.Core.Events;
using Neimart.Core.Filters;
using Neimart.Core.Utilities;
using Neimart.Core.Utilities.Helpers;

namespace Neimart.Core.Services
{
    public class CategoryService : EntityService<Category, CategoryFilter>
    {
        public CategoryService(IServiceProvider services) : base(services)
        {
        }

        public override async Task CreateAsync(Category category)
        {
            if (category == null)
                throw new ArgumentNullException(nameof(category));

            category.CreatedOn = DateTimeOffset.UtcNow;
            category.PublishedOn = category.Published ? DateTimeOffset.UtcNow : default;

            await GenerateSlugAsync(category);
            await base.CreateAsync(category);
        }

        public override async Task UpdateAsync(Category category)
        {
            if (category == null)
                throw new ArgumentNullException(nameof(category));

            category.Updated = true;
            category.UpdatedOn = DateTimeOffset.UtcNow;
            category.PublishedOn = category.Published && category.PublishedOn == default ? DateTimeOffset.UtcNow : category.Published ? category.PublishedOn : default;

            await GenerateSlugAsync(category);
            await base.UpdateAsync(category);
        }

        public override Task DeleteAsync(Category category)
        {
            if (category == null)
                throw new ArgumentNullException(nameof(category));

            category.Deleted = true;
            return UpdateAsync(category);
        }

        public override async Task ClearCacheAsync()
        {
            await _cacheManager.RemoveByPrefixAsync(EntityHelper.GetCachePrefix<Product>());
            await _cacheManager.RemoveByPrefixAsync(EntityHelper.GetCachePrefix<Cart>());
            await base.ClearCacheAsync();
        }

        public override async Task<bool> ExistsAsync(Category category)
        {
            var query = _unitOfWork.Query<Category>();
            return await query.AnyAsync(x => x.Id == category.Id || x.Slug == category.Slug);
        }

        public override IQueryable<Category> GetQuery(CategoryFilter filter = null)
        {
            var query = _unitOfWork.Query<Category>();

            query = query.Include(x => x.Image)
                         .Include(x => x.Tags);

            query = query.OrderByDescending(x => !x.Updated ? x.CreatedOn : x.UpdatedOn);

            if (filter != null)
            {
                if (filter.SellerId != null)
                {
                    query = query.Where(x => x.SellerId == filter.SellerId);
                }

                if (filter.CategoryId != null)
                {
                    query = query.Where(x => x.Id == filter.CategoryId);
                }

                if (filter.Slug != null)
                {
                    query = query.Where(x => !string.IsNullOrWhiteSpace(x.Slug) && x.Slug == filter.Slug);
                }

                if (filter.Published != null)
                {
                    query = query.Where(x => x.Published == filter.Published);
                }

                if (filter.ImageRequired != null)
                {
                    query = query.Where(x => filter.ImageRequired.Value && x.Image != null);
                }

                if (!string.IsNullOrWhiteSpace(filter.Search))
                {
                    query = query.Where(x =>
                    EF.Functions.Like(x.Name, $"%{filter.Search}%") ||
                    EF.Functions.Like(x.Slug, $"%{filter.Search}%") ||
                    x.Tags.Any(tag => EF.Functions.Like(tag.Name, $"%{filter.Search}%") ||
                    x.Tags.Any(tag => EF.Functions.Like(tag.Slug, $"%{filter.Search}%"))));
                }
            }

            return query;
        }

        public Task GenerateSlugAsync(Category category, bool uniqueness = true)
        {
            bool slugExists(string slug) => GetQuery().Any(x => x.SellerId == category.SellerId && x.Id != category.Id && x.Slug == slug);

            category.Slug = SanitizerHelper.GenerateSlug(category.Name,
             uniqueness ? (Func<string, bool>)slugExists : null);

            return Task.CompletedTask;
        }

        public async Task<CountEvaluation> EvaluateAsync(CategoryFilter filter)
        {
            if (filter == null)
                throw new ArgumentNullException(nameof(filter));

            async Task<CountEvaluation> acquire()
            {
                var query = GetQuery(filter);

                return InternalEvaluate(count: await query.CountAsync());
            }

            string key = await _cacheManager.ComposeKeyAsync($"{EntityHelper.GetCachePrefix<Category>()}.{nameof(EvaluateAsync)}", filter);
            var result = await _cacheManager.GetAsync(key, acquire);
            return result;
        }
    }
}