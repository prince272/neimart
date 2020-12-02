using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using Neimart.Core.Entities;
using Neimart.Core.Filters;
using Neimart.Core.Utilities.Extensions;

namespace Neimart.Core.Services
{
    public class OrderService : EntityService<Order, OrderFilter>
    {
        public OrderService(IServiceProvider services)
            : base(services)
        {
        }

        public override async Task CreateAsync(Order order)
        {
            if (order == null)
                throw new ArgumentNullException(nameof(order));

            order.CreatedOn = DateTimeOffset.UtcNow;

            await base.CreateAsync(order);
        }

        public override async Task UpdateAsync(Order order)
        {
            if (order == null)
                throw new ArgumentNullException(nameof(order));

            order.Updated = true;
            order.UpdatedOn = DateTimeOffset.UtcNow;

            await base.UpdateAsync(order);
        }

        public override Task DeleteAsync(Order order)
        {
            if (order == null)
                throw new ArgumentNullException(nameof(order));

            order.Deleted = true;
            return UpdateAsync(order);
        }

        public override async Task ClearCacheAsync()
        {
            await _cacheManager.RemoveByPrefixAsync(EntityHelper.GetCachePrefix<OrderItem>());
            await base.ClearCacheAsync();
        }

        public override IQueryable<Order> GetQuery(OrderFilter filter = null)
        {
            var query = _unitOfWork.Query<Order>();
            query = query.Include(x => x.Seller).ThenInclude(x => x.UserImage);
            query = query.Include(x => x.Customer).ThenInclude(x => x.UserImage);
            query = query.Include(x => x.OrderItems);

            query = query.OrderByDescending(x => !x.Updated ? x.CreatedOn : x.UpdatedOn);

            if (filter != null)
            {
                if (filter.SellerId != null)
                {
                    query = query.Where(x => x.SellerId == filter.SellerId);
                }

                if (filter.CustomerId != null)
                {
                    query = query.Where(x => x.CustomerId == filter.CustomerId);
                }

                if (filter.OrderId != null)
                {
                    query = query.Where(x => x.Id == filter.OrderId);
                }

                if (filter.TrackingCode != null)
                {
                    query = query.Where(x => !string.IsNullOrWhiteSpace(x.TrackingCode) && x.TrackingCode == filter.TrackingCode);
                }

                if (filter.OrderCode != null)
                {
                    query = query.Where(x => !string.IsNullOrWhiteSpace(x.OrderCode) && x.OrderCode == filter.OrderCode);
                }

                if (filter.Status != null)
                {
                    query = query.Where(x => x.Status == filter.Status);
                }

                if (filter.Paid != null)
                {
                    query = query.Where(x => x.Paid == filter.Paid);
                }

                if (!string.IsNullOrWhiteSpace(filter.Search))
                {
                    query = query.Where(x =>
                    (EF.Functions.Like(x.OrderCode, $"%{filter.Search}%") ||
                    (EF.Functions.Like(x.TrackingCode, $"%{filter.Search}%"))));
                }
            }

            return query;
        }

        public Task<Result> CanChangeStatusAsync(Order order, OrderStatus status)
        {
            bool success;
            Result result;

            string canNotChangeText = $"Cannot change the status of order to {status.GetEnumText().ToLowerInvariant()}";

            if (status == OrderStatus.Processing)
            {
                success = order.Status == OrderStatus.Pending && order.DeliveryRequired;
                result = !success ? Result.Fail($"{canNotChangeText} because it's {order.Status.GetEnumText().ToLowerInvariant()} and doesn't require delivery.") : Result.Ok();
            }
            else if (status == OrderStatus.Delivering)
            {
                success = order.Status == OrderStatus.Processing && order.DeliveryRequired;
                result = !success ? Result.Fail($"{canNotChangeText} because it's {order.Status.GetEnumText().ToLowerInvariant()} and doesn't require delivery.") : Result.Ok();
            }
            else if (status == OrderStatus.Complete)
            {
                success = order.Status == OrderStatus.Delivering && order.DeliveryRequired ||
                          order.Status == OrderStatus.Pending && !order.DeliveryRequired;
                result = !success ? Result.Fail($"{canNotChangeText} because it's {order.Status.GetEnumText().ToLowerInvariant()} and {(order.DeliveryRequired ? "requires shipping." : "doesn't require shipping")}.") : Result.Ok();
            }
            else if (status == OrderStatus.Cancelled)
            {
                success = (order.Status != OrderStatus.Cancelled);
                result = !success ? Result.Fail($"{canNotChangeText} because it's {order.Status.GetEnumText().ToLowerInvariant()}.") : Result.Ok();
            }
            else
            {
                result = Result.Fail(canNotChangeText + ".");
            }

            return Task.FromResult(result);
        }

        protected override Task PrepareAsync(Order order)
        {
            if (order.Status == OrderStatus.Complete) order.CompletedOn = DateTimeOffset.UtcNow;
            else if (order.Status == OrderStatus.Delivering) order.DeliveringOn = DateTimeOffset.UtcNow;
            else if (order.Status == OrderStatus.Cancelled) order.CancelledOn = DateTimeOffset.UtcNow;

            order.PaidOn = order.Paid && order.PaidOn == default ? DateTimeOffset.UtcNow : order.Paid ? order.PaidOn : default;

            return Task.CompletedTask;
        }
    }
}