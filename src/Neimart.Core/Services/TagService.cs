using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;
using Neimart.Core.Entities;
using Neimart.Core.Events;
using Neimart.Core.Filters;

namespace Neimart.Core.Services
{
    public class TagService : EntityService<Tag, TagFilter>
    {
        public TagService(IServiceProvider services) : base(services)
        {
        }

        public override async Task ClearCacheAsync()
        {
            await _cacheManager.RemoveByPrefixAsync(EntityHelper.GetCachePrefix<Product>());
            await base.ClearCacheAsync();
        }

        public override IQueryable<Tag> GetQuery(TagFilter filter = null)
        {
            var query = _unitOfWork.Query<Tag>();

            query = query.OrderByDescending(x => x);

            if (filter != null)
            {
                if (filter.SellerId != null)
                {
                    query = query.Where(x => x.Product != null && x.Product.SellerId == filter.SellerId || 
                                             x.Category != null && x.Category.SellerId == filter.SellerId);
                }
            }

            return query;
        }
    }
}
