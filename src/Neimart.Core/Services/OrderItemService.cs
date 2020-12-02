using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.EventSource;
using Neimart.Core.Entities;
using Neimart.Core.Filters;

namespace Neimart.Core.Services
{
    public class OrderItemService : EntityService<OrderItem, OrderItemFilter>
    {
        public OrderItemService(IServiceProvider services) : base(services)
        {
        }

        public override IQueryable<OrderItem> GetQuery(OrderItemFilter filter = null)
        {
            var query = _unitOfWork.Query<OrderItem>();

            query = query.Include(x => x.Order);

            query = query.OrderByDescending(x => x);

            if (filter != null)
            {
                if (filter.OrderId != null)
                {
                    query = query.Where(x => x.OrderId == filter.OrderId);
                }

                if (filter.OrderIds != null)
                {
                    query = query.Where(x => filter.OrderIds.Contains(x.OrderId));
                }

                if (filter.OrderItemId != null)
                {
                    query = query.Where(x => x.Id == filter.OrderItemId);
                }

                if (filter.OrderItemIds != null)
                {
                    query = query.Where(x => filter.OrderItemIds.Contains(x.Id));
                }

                if (filter.DocumentRequired != null)
                {
                    if (filter.DocumentRequired.Value) query = query.Where(x => x.Document != null);
                    else query = query.Where(x => x.Document == null);
                }
            }

            return query;
        }

        public override async Task ClearCacheAsync()
        {
            await _cacheManager.RemoveByPrefixAsync(EntityHelper.GetCachePrefix<Order>());
            await base.ClearCacheAsync();
        }
    }
}
