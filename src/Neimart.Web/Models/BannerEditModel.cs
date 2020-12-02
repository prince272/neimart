using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using FluentValidation;
using Neimart.Core;
using Neimart.Core.Entities;
using Neimart.Core.Infrastructure.Imaging;
using Neimart.Core.Infrastructure.Web;
using Neimart.Core.Utilities.Helpers;

namespace Neimart.Web.Models
{
    public class BannerEditModel
    {
        public bool IsEdit => Id != 0;

        public long Id { get; set; }

        public string Title { get; set; }

        public string Permalink { get; set; }

        [Display(Description = "Indicate whether this banner is published in your store.")]
        public bool Published { get; set; }

        public BannerSize Size { get; set; }

        public BannerImage Image { get; set; }

        public ImageResizeInfo ImageResize
        {
            get
            {
                var imageSizeInfo = AttributeHelper
                    .GetMemberAttribute<ImageSizeAttribute>(typeof(BannerSize).GetMember(Size.ToString())[0]);

                return new ImageResizeInfo
                {
                    Format = ImageFormat.Jpg,
                    Width = imageSizeInfo.Width,
                    Height = imageSizeInfo.Height,
                    Mode = ImageMode.CropScale
                };
            }
        }
    }

    public class BannerEditProfile : Profile
    {
        public BannerEditProfile()
        {
            CreateMap<BannerEditModel, Banner>()
                .ForMember(x => x.Image, x => x.Ignore())
                .ForMember(x => x.ImageId, x => x.Ignore());
            CreateMap<Banner, BannerEditModel>();
        }
    }

    public class BannerEditValidator : AbstractValidator<BannerEditModel>
    {
        public BannerEditValidator()
        {
            RuleFor(x => x.Title).Text();
        }
    }
}
