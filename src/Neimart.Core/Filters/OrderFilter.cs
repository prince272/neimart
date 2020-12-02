using System;
using System.Collections.Generic;
using System.Text;
using Neimart.Core.Entities;

namespace Neimart.Core.Filters
{
    public class OrderFilter
    {
        public long? SellerId { get; set; }

        public long? CustomerId { get; set; }

        public string OrderCode { get; set; }

        public string TrackingCode { get; set; }

        public long? OrderId { get; set; }

        public bool? DeliveryRequired { get; set; }

        public OrderStatus? Status { get; set; }

        public bool? Paid { get; set; }

        public string Search { get; set; }
    }
}
