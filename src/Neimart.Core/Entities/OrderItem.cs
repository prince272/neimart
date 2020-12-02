using System;
using System.Collections.Generic;
using System.Text;

namespace Neimart.Core.Entities
{
    public class OrderItem : IEntity
    {
        public virtual Order Order { get; set; }

        public long OrderId { get; set; }

        public long Id { get; set; }

        public long ProductId { get; set; }

        public string Slug { get; set; }

        public decimal Price { get; set; }

        public decimal Amount => Price * Quantity;

        public int Quantity { get; set; }

        public string Name { get; set; }

        public Media Image { get; set; }

        public Media Document { get; set; }

        public string Sku { get; set; }
    }
}
