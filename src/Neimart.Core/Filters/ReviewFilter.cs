using System;
using System.Collections.Generic;
using System.Text;

namespace Neimart.Core.Filters
{
    public class ReviewFilter
    {
        public long? ProductId { get; set; }

        public long? CustomerId { get; set; }

        public long? SellerId { get; set; }

        public int? Rating { get; set; }

        public bool? Approved { get; set; }

        public string Title { get; set; }

        public long? ReviewId { get; set; }
    }
}
