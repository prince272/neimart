using System;
using System.Collections.Generic;
using System.Text;

namespace Neimart.Core.Filters
{
    public class ActivityFilter
    {
        public long? SellerId { get; set; }

        public long? ActivityId { get; set; }

        public string EntityName { get; set; }

        public string IpAddress { get; set; }

        public long[] ActivityIds { get; set; }
    }
}
