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
    public class CartService : EntityService<Cart, CartFilter>
    {
        public CartService(IServiceProvider services) : base(services)
        {
        }

        public override Task CreateAsync(Cart cart)
        {
            if (cart == null)
                throw new ArgumentNullException(nameof(cart));

            cart.CreatedOn = DateTimeOffset.UtcNow;

            return base.CreateAsync(cart);
        }

        public override Task UpdateAsync(Cart cart)
        {
            if (cart == null)
                throw new ArgumentNullException(nameof(cart));

            cart.Updated = true;
            cart.UpdatedOn = DateTimeOffset.UtcNow;

            return base.UpdateAsync(cart);
        }

        public override IQueryable<Cart> GetQuery(CartFilter filter = null)
        {
            var query = _unitOfWork.Query<Cart>();
            query = query.Include(x => x.Seller);
            query = query.Include(x => x.Customer);
            query = query.Include(x => x.Product).ThenInclude(x => x.Images);
            query = query.Include(x => x.Product).ThenInclude(x => x.Document);
            query = query.Include(x => x.Product).ThenInclude(x => x.Tags);

            query = query.OrderByDescending(x => !x.Updated ? x.CreatedOn : x.UpdatedOn);

            if (filter != null)
            {
                if (filter.CustomerId != null)
                {
                    query = query.Where(x => x.CustomerId == filter.CustomerId);
                }

                if (filter.SellerId != null)
                {
                    query = query.Where(x => x.SellerId == filter.SellerId);
                }

                if (filter.ProductId != null)
                {
                    query = query.Where(x => x.ProductId == filter.ProductId);
                }

                if (filter.Type != null)
                {
                    query = query.Where(x => x.Type == filter.Type);
                }
            }

            return query;
        }

        public ValueTask<AmountEvaluation> EvaluateAsync(Cart cart)
        {
            if (cart == null)
                throw new ArgumentNullException(nameof(cart));

            return new ValueTask<AmountEvaluation>(InternalEvaluate(
                amount: cart.Product.Price * cart.Quantity,
                quantity: cart.Quantity));
        }

        public ValueTask<AmountEvaluation> EvaluateAsync(IEnumerable<Cart> carts)
        {
            if (carts == null)
                throw new ArgumentNullException(nameof(carts));

            return new ValueTask<AmountEvaluation>(InternalEvaluate(
                    amount: carts.Select(x => x.Product.Price * x.Quantity).Sum(),
                    quantity: carts.Select(x => x.Quantity).Sum(),
                    count: carts.Count()));
        }

        public Result Validate(Cart cart)
        {
            if (cart.Product.Stock == ProductStock.OutOfStock)
                return Result.Fail("This product is out of stock and cannot be added to cart.");

            else if (cart.Product.Free)
                return Result.Fail("This product is free and cannot be added to cart.");

            return Result.Ok();
        }
    }
}