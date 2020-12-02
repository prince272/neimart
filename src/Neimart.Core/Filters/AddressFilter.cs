using System;
using System.Collections.Generic;
using System.Text;
using Neimart.Core.Entities;

namespace Neimart.Core.Filters
{
    public class AddressFilter
    {
        public long? CustomerId { get; set; }

        public long? AddressId { get; set; }

        public AddressType? AddressType { get; set; }

        public AddressType[] AddressTypes { get; set; }
    }
}
