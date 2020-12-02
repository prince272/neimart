using System;
using System.Collections.Generic;
using System.Text;

namespace Neimart.Core.Entities
{
    public class ProductAttribute : IEntity
    {
        public virtual Product Product { get; set; }

        public long ProductId { get; set; }

        public long Id { get; set; }

        public string Name { get; set; }

        public string Value { get; set; }
    }
}