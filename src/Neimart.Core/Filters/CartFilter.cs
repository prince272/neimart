using System;
using System.Collections.Generic;
using System.Text;
using Neimart.Core.Entities;

namespace Neimart.Core.Filters
{
    public class CartFilter
    {
        public long? ProductId { get; set; }

        public CartType? Type { get; set; }

        public long? CustomerId { get; set; }

        public long? SellerId { get; set; }
    }
}