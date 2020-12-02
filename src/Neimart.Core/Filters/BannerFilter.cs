using System;
using System.Collections.Generic;
using System.Text;
using Neimart.Core.Entities;

namespace Neimart.Core.Filters
{
    public class BannerFilter
    {
        public long? SellerId { get; set; }

        public long? BannerId { get; set; }

        public string Permalink { get; set; }

        public bool? Published { get; set; }

        public string Search { get; set; }

        public BannerSize? Size { get; set; }
    }
}
