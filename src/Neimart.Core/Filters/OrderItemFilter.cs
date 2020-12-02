using System;
using System.Collections.Generic;
using System.Text;

namespace Neimart.Core.Filters
{
    public class OrderItemFilter
    {
        public long? OrderId { get; set; }

        public long[] OrderIds { get; set; }

        public bool? DocumentRequired { get; set; }

        public long? OrderItemId { get; set; }

        public long[] OrderItemIds { get; set; }
    }
}
