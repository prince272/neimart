using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using FluentValidation;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Options;
using Neimart.Core;
using Neimart.Core.Entities;
using Neimart.Core.Infrastructure.Imaging;
using Neimart.Core.Infrastructure.Web;
using Neimart.Core.Services;
using Neimart.Core.Settings;
using SkiaSharp;

namespace Neimart.Web.Models
{
    public class ProductEditModel
    {
        public long Id { get; set; }

        public bool IsEdit => Id != 0;

        public string Name { get; set; }

        public string Description { get; set; }


        [Display(Description = "Indicate whether this product is published in your store.")]
        public bool Published { get; set; }

        public string Sku { get; set; }

        public decimal Weight { get; set; }

        public ProductStock Stock { get; set; }

        public List<SelectListItem> StockOptions { get; } = new List<SelectListItem>();

        public decimal? Price { get; set; }

        public decimal OldPrice { get; set; }

        public decimal Cost { get; set; }

        [Display(Name = "Tags")]
        public List<string> TagNames { get; } = new List<string>();

        public List<SelectListItem> TagOptions { get; } = new List<SelectListItem>();

        public List<ProductImage> Images { get; set; } = new List<ProductImage>();

        public ImageResizeInfo ImageResize
        {
            get
            {
                return new ImageResizeInfo
                {
                    Format = ImageFormat.Jpg,
                    Width = 992,
                    Height = 992,
                    Mode = ImageMode.CropScale
                };
            }
        }

        public ProductDocument Document { get; set; }
    }

    public class ProductEditProfile : Profile
    {
        public ProductEditProfile()
        {
            CreateMap<ProductEditModel, Product>()
                .ForMember(x => x.Images, x => x.Ignore())
                .ForMember(x => x.Document, x => x.Ignore())
                .ForMember(x => x.DocumentId, x => x.Ignore());

            CreateMap<Product, ProductEditModel>();
        }
    }

    public class ProductEditValidator : AbstractValidator<ProductEditModel>
    {
        public ProductEditValidator(IOptions<AppSettings> appSettingsAccessor)
        {
            RuleFor(x => x.Name).NotNull().NotEmpty().Name();
            RuleFor(x => x.Sku).Text();
            RuleFor(x => x.Price).NotNull().InclusiveBetween(0, appSettingsAccessor.Value.CurrencyMaxValue);
            RuleFor(x => x.OldPrice).InclusiveBetween(0, appSettingsAccessor.Value.CurrencyMaxValue);
            RuleFor(x => x.Cost).InclusiveBetween(0, appSettingsAccessor.Value.CurrencyMaxValue);
        }
    }
}
