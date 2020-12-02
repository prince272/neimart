using System;
using System.Collections.Generic;
using System.Text;

namespace Neimart.Core.Entities
{
    public class ReviewReaction : IEntity
    {
        public virtual Review Review { get; set; }

        public long ReviewId { get; set; }

        public long Id { get; set; }

        public bool Helpful { get; set; }

        public virtual User Customer { get; set; }

        public long CustomerId { get; set; }
    }
}
