using System;
using System.Collections.Generic;
using System.Text;

namespace Neimart.Core.Entities
{
    public class ProductImage : Media
    {
        public virtual Product Product { get; set; }

        public long ProductId { get; set; }
    }
}
