using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Neimart.Core.Entities
{
    public class Cart : IEntity
    {
        public virtual User Customer { get; set; }

        public long CustomerId { get; set; }

        public virtual User Seller { get; set; }

        public long SellerId { get; set; }

        public long Id { get; set; }

        public CartType Type { get; set; }

        public int Quantity { get; set; }

        public virtual Product Product { get; set; }

        public long ProductId { get; set; }

        public DateTimeOffset CreatedOn { get; set; }

        public DateTimeOffset UpdatedOn { get; set; }

        public bool Updated { get; set; }
    }

    public enum CartType
    {
        Cart,
        Wishlist
    }
}