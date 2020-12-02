using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using FluentValidation;
using Microsoft.AspNetCore.Mvc.Rendering;
using Neimart.Core;
using Neimart.Core.Entities;
using Neimart.Core.Infrastructure.Imaging;
using Neimart.Core.Infrastructure.Web;

namespace Neimart.Web.Models
{
    public class StoreEditModel
    {
        public string StoreName { get; set; }

        public string StoreDescription { get; set; }

        [Display(Name = "Store url")]
        public string StoreSlug { get; set; }

        public UserDocument StoreDocument { get; set; }

        public StoreStatus StoreStatus { get; set; }

        [Display(Name = "Store delivery")]
        public bool StoreDeliveryRequired { get; set; }

        public List<DeliveryEditModel> StoreDeliveries { get; set; } = new List<DeliveryEditModel>();

        public List<SelectListItem> StoreDeliveryRequiredOptions { get; } = new List<SelectListItem>();

        public List<StoreCategory> StoreCategorySelections { get; } = new List<StoreCategory>();

        public List<SelectListItem> StoreCategoryOptions { get; } = new List<SelectListItem>();

        public List<SelectListItem> StoreStatusOptions { get; } = new List<SelectListItem>();

        public UserImage StoreLogo { get; set; }

        public ImageResizeInfo StoreLogoResize
        {
            get
            {
                return new ImageResizeInfo
                {
                    Format = ImageFormat.Jpg,
                    Width = 256,
                    Height = 256,
                    Mode = ImageMode.CropScale
                };
            }
        }

        public ThemeMode StoreThemeMode { get; set; }

        public List<SelectListItem> StoreThemeModeOptions { get; } = new List<SelectListItem>();

        public ThemeStyle StoreThemeStyle { get; set; }

        public List<SelectListItem> StoreThemeStyleOptions { get; } = new List<SelectListItem>();

        public string AboutNote { get; set; }

        public string TermsNote { get; set; }

        public string PrivacyNote { get; set; }

        public string ReturnsNote { get; set; }

        public string ReviewsNote { get; set; }

        public string StoreRegion { get; set; }

        public string StorePlace { get; set; }

        [Display(Name = "Store digital/postal address")]
        public string StorePostal { get; set; }

        [Display(Name = "Street, apartment, suite, unit")]
        public string StoreStreet { get; set; }


        public string FacebookLink { get; set; }

        public string TwitterLink { get; set; }

        public string YoutubeLink { get; set; }

        public string InstagramLink { get; set; }

        public string LinkedInLink { get; set; }

        public string PinterestLink { get; set; }

        [Display(Name = "WhatsApp number")]
        public string WhatsAppNumber { get; set; }

        public string MapLink { get; set; }
    }

    public class StoreEditProfile : Profile
    {
        public StoreEditProfile()
        {
            CreateMap<StoreEditModel, User>()
                .ForMember(x => x.StoreLogoId, x => x.Ignore())
                .ForMember(x => x.StoreLogo, x => x.Ignore());

            CreateMap<User, StoreEditModel>();
        }
    }

    public class StoreEditValidator : AbstractValidator<StoreEditModel>
    {
        public StoreEditValidator(IServiceProvider services)
        {
            RuleFor(x => x.StoreName).NotNull().NotEmpty().Name();
            RuleFor(x => x.StoreSlug).NotNull().NotEmpty().Slug();

            RuleFor(x => x.StoreRegion).NotNull().NotEmpty();
            RuleFor(x => x.StorePlace).NotNull().NotEmpty();
            RuleFor(x => x.StoreStreet).NotNull().NotEmpty();

            RuleForEach(x => x.StoreDeliveries).SetValidator(new DeliveryEditValidator(services));

            RuleFor(x => x.FacebookLink).Url();
            RuleFor(x => x.TwitterLink).Url();
            RuleFor(x => x.YoutubeLink).Url();
            RuleFor(x => x.InstagramLink).Url();
            RuleFor(x => x.LinkedInLink).Url();
            RuleFor(x => x.PinterestLink).Url();
            RuleFor(x => x.WhatsAppNumber).Phone();
            RuleFor(x => x.MapLink).Url();
        }
    }
}
