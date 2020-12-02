using System;
using System.Collections.Generic;
using System.Text;

namespace Neimart.Core.Entities
{
    public class Tag : IEntity
    {
        public virtual Category Category { get; set; }

        public long? CategoryId { get; set; }

        public virtual Product Product { get; set; }

        public long? ProductId { get; set; }

        public long Id { get; set; }

        public string Name { get; set; }

        public string Slug { get; set; }
    }
}
