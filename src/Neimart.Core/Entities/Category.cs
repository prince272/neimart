using System;
using System.Collections.Generic;
using System.Text;

namespace Neimart.Core.Entities
{
    public class Category : IEntity
    {
        public virtual User Seller { get; set; }

        public long SellerId { get; set; }

        public long Id { get; set; }

        public string Name { get; set; }

        public string Icon { get; set; }

        public string Slug { get; set; }

        public DateTimeOffset CreatedOn { get; set; }

        public DateTimeOffset UpdatedOn { get; set; }

        public bool Updated { get; set; }

        public DateTimeOffset PublishedOn { get; set; }

        public bool Published { get; set; }

        public bool Deleted { get; set; }

        public virtual CategoryImage Image { get; set; }

        public long? ImageId { get; set; }

        public virtual ICollection<Tag> Tags { get; set; } = new List<Tag>();
    }
}
