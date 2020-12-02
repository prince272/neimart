using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Neimart.Core.Entities;
using Neimart.Core.Filters;

namespace Neimart.Core.Services
{
    public class BannerService : EntityService<Banner, BannerFilter>
    {
        public BannerService(IServiceProvider services) : base(services)
        {
        }

        public override Task CreateAsync(Banner banner)
        {
            if (banner == null)
                throw new ArgumentNullException(nameof(banner));

            banner.CreatedOn = DateTimeOffset.UtcNow;
            banner.PublishedOn = banner.Published ? DateTimeOffset.UtcNow : default;

            return base.CreateAsync(banner);
        }

        public override Task UpdateAsync(Banner banner)
        {
            if (banner == null)
                throw new ArgumentNullException(nameof(banner));

            banner.Updated = true;
            banner.UpdatedOn = DateTimeOffset.UtcNow;
            banner.PublishedOn = banner.Published && banner.PublishedOn == default ? DateTimeOffset.UtcNow : banner.Published ? banner.PublishedOn : default;

            return base.UpdateAsync(banner);
        }

        public override IQueryable<Banner> GetQuery(BannerFilter filter = null)
        {
            var query = _unitOfWork.Query<Banner>();

            query = query.Include(x => x.Image);

            query = query.OrderByDescending(x => !x.Updated ? x.CreatedOn : x.UpdatedOn);

            if (filter != null)
            {
                if (filter.SellerId != null)
                {
                    query = query.Where(x => x.SellerId == filter.SellerId);
                }

                if (filter.BannerId != null)
                {
                    query = query.Where(x => x.Id == filter.BannerId);
                }

                if (filter.Permalink != null)
                {
                    query = query.Where(x => !string.IsNullOrWhiteSpace(x.Permalink) && x.Permalink == filter.Permalink);
                }

                if (filter.Published != null)
                {
                    query = query.Where(x => x.Published == filter.Published);
                }

                if (filter.Size != null)
                {
                    query = query.Where(x => x.Size == filter.Size);
                }

                if (!string.IsNullOrWhiteSpace(filter.Search))
                {
                    query = query.Where(x =>
                    EF.Functions.Like(x.Title, $"%{filter.Search}%") ||
                    EF.Functions.Like(x.Permalink, $"%{filter.Search}%"));
                }
            }

            return query;
        }

        public async Task<CountEvaluation> EvaluateAsync(BannerFilter filter)
        {
            if (filter == null)
                throw new ArgumentNullException(nameof(filter));

            async Task<CountEvaluation> acquire()
            {
                var query = GetQuery(filter);

                return InternalEvaluate(count: await query.CountAsync());
            }

            string key = await _cacheManager.ComposeKeyAsync($"{EntityHelper.GetCachePrefix<Banner>()}.{nameof(EvaluateAsync)}", filter);
            var result = await _cacheManager.GetAsync(key, acquire);
            return result;
        }
    }
}
