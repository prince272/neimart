using System;
using System.Collections.Generic;
using System.Text;

namespace Neimart.Core.Entities
{
    public class Review : IEntity
    {
        public virtual Product Product { get; set; }

        public long ProductId { get; set; }

        public long Id { get; set; }

        public string Title { get; set; }

        public string Comment { get; set; }

        public string Reply { get; set; }

        public int Rating { get; set; }

        public DateTimeOffset CreatedOn { get; set; }

        public DateTimeOffset UpdatedOn { get; set; }

        public bool Updated { get; set; }

        public virtual User Customer { get; set; }

        public long CustomerId { get; set; }

        public bool Approved { get; set; }

        public virtual ICollection<ReviewReaction> Reactions { get; set; } = new List<ReviewReaction>();
    }
}