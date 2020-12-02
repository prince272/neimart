using System;
using System.Collections.Generic;
using System.Text;
using Neimart.Core.Entities;

namespace Neimart.Core.Filters
{
    public class ProductFilter : CategoryFilter
    {
        public long? ProductId { get; set; }

        public bool? DocumentRequired { get; set; }

        public ProductStock? Stock { get; set; }

        public ProductSort? Sort { get; set; }

        public decimal? MinPrice { get; set; }

        public decimal? MaxPrice { get; set; }

        public int? Rating { get; set; }

        public decimal? Discount { get; set; }
    }

    public enum ProductSort
    {
        Popular,
        Newest,
        Oldest,
        HighestPrice,
        LowestPrice,
        Updated,
    }
}
