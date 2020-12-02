using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using Neimart.Core.Utilities.Helpers;

namespace Neimart.Core.Entities
{
    public class Banner : IEntity
    {
        public virtual User Seller { get; set; }

        public long SellerId { get; set; }

        public long Id { get; set; }

        public string Title { get; set; }

        public string Permalink { get; set; }

        public BannerSize Size { get; set; }

        public DateTimeOffset CreatedOn { get; set; }

        public DateTimeOffset UpdatedOn { get; set; }

        public bool Updated { get; set; }

        public DateTimeOffset PublishedOn { get; set; }

        public bool Published { get; set; }

        public bool Deleted { get; set; }

        public virtual BannerImage Image { get; set; }

        public long? ImageId { get; set; }
    }

    public enum BannerSize
    {
        [ImageSize(Width = 1140, Height = 374)]
        [Display(Name = "Large (1140 x 374)")]
        Large,

        [ImageSize(Width = 960, Height = 522)]
        [Display(Name = "Medium (960 x 522)")]
        Medium,

        [ImageSize(Width = 540, Height = 360)]
        [Display(Name = "Small (540 x 360)")]
        Small,
    }

    [AttributeUsage(AttributeTargets.Field)]
    public sealed class ImageSizeAttribute : Attribute
    {
        public int Width { get; set; }

        public int Height { get; set; }
    }
}
