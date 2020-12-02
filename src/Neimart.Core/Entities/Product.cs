using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Humanizer;
using Neimart.Core.Utilities.Helpers;

namespace Neimart.Core.Entities
{
    public class Product : IEntity
    {
        public virtual User Seller { get; set; }

        public long SellerId { get; set; }

        public long Id { get; set; }

        public string Name { get; set; }

        public string Description { get; set; }

        public string PlainDescription => SanitizerHelper.ConvertHtmlToText(Description).Truncate(255, Truncator.FixedLength);

        public DateTimeOffset CreatedOn { get; set; }

        public DateTimeOffset UpdatedOn { get; set; }

        public bool Updated { get; set; }

        public DateTimeOffset PublishedOn { get; set; }

        public bool Published { get; set; }

        public bool Deleted { get; set; }

        public string Slug { get; set; }

        public string Sku { get; set; }

        public ProductStock Stock { get; set; }

        public bool Free => Price <= 0;

        public decimal Price { get; set; }

        public decimal OldPrice { get; set; }

        public decimal Cost { get; set; }

        public virtual ProductDocument Document { get; set; }

        public long? DocumentId { get; set; }

        public ProductImage Image => Images.FirstOrDefault();

        public virtual List<Tag> Tags { get; set; } = new List<Tag>();

        private ICollection<ProductImage> _images = new List<ProductImage>();
        public virtual ICollection<ProductImage> Images { get => _images.OrderBy(x => x.Position).ToList(); set => _images = value; }

        public virtual ICollection<Review> Reviews { get; set; } = new List<Review>();

        public virtual ICollection<ProductAttribute> Attributes { get; set; } = new List<ProductAttribute>();

        public bool DiscountAvailable => GetDiscount().discountAvailable;

        public decimal DiscountPercent => GetDiscount().discountPercent;

        public decimal SavingAmount => GetDiscount().savingAmount;

        public (bool discountAvailable, decimal discountPercent, decimal savingAmount) GetDiscount()
        {
            bool discountAvailable = false;
            decimal discountPercent = 0;
            decimal savingAmount = 0;

            if (OldPrice > 0 && OldPrice > Price)
            {
                discountPercent = (100 - Math.Ceiling((Price / OldPrice) * 100));
                savingAmount = OldPrice - Price;
                discountAvailable = true;
            }

            return (discountAvailable, discountPercent, savingAmount);
        }
    }

    public enum ProductStock
    {
        OutOfStock,
        InStock
    }
}
