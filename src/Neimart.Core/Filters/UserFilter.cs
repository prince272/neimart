using System;
using System.Collections.Generic;
using System.Text;
using Neimart.Core.Entities;

namespace Neimart.Core.Filters
{
    public class UserFilter
    {
        public bool? StoreSetup { get; set; }

        public string StoreRegion { get; set; }

        public string StorePlace { get; set; }

        public string Search { get; set; }

        public StoreCategory? StoreCategory { get; set; }

        public StoreStatus? StoreStatus { get; set; }

        public StoreAccess? StoreAccess { get; set; }

       public bool? StorePlanActive { get; set; }
    }
}