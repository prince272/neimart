using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using FluentValidation;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Options;
using Neimart.Core.Entities;
using Neimart.Core.Infrastructure.Imaging;
using Neimart.Core.Infrastructure.Web;

namespace Neimart.Web.Models
{
    public class CategoryEditModel
    {
        public bool IsEdit => Id != 0;

        public long Id { get; set; }

        public string Name { get; set; }

        public string Icon { get; set; }

        public List<SelectListItem> IconOptions { get; } = new List<SelectListItem>();

        [Display(Description = "Indicate whether this category is published in your store.")]
        public bool Published { get; set; }

        public CategoryImage Image { get; set; }

        public ImageResizeInfo ImageResize
        {
            get
            {
                return new ImageResizeInfo
                {
                    Format = ImageFormat.Jpg,
                    Width = 184,
                    Height = 184,
                    Mode = ImageMode.CropScale
                };
            }
        }

        [Display(Name = "Tags")]
        public List<string> TagNames { get; } = new List<string>();

        public List<SelectListItem> TagOptions { get; } = new List<SelectListItem>();
    }

    public class CategoryEditProfile : Profile
    {
        public CategoryEditProfile()
        {
            CreateMap<CategoryEditModel, Category>()
                .ForMember(x => x.Image, x => x.Ignore())
                .ForMember(x => x.ImageId, x => x.Ignore());

            CreateMap<Category, CategoryEditModel>();
        }
    }

    public class CategoryEditValidator : AbstractValidator<CategoryEditModel>
    {
        public CategoryEditValidator()
        {
            RuleFor(x => x.Name).NotNull().NotEmpty().Name();
        }
    }
}
