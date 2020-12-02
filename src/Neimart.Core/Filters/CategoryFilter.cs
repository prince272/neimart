using System;
using System.Collections.Generic;
using System.Text;

namespace Neimart.Core.Filters
{
    public class CategoryFilter
    {
        public long? SellerId { get; set; }

        public long? CategoryId { get; set; }

        public string Slug { get; set; }

        public string Search { get; set; }

        public bool? Published { get; set; }

        public bool? ImageRequired { get; set; }
    }
}
