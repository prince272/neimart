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

namespace Neimart.Web.Models
{
    public class ProfileEditModel
    {
        public string FirstName { get; set; }

        public string LastName { get; set; }

        public string Bio { get; set; }

        public string Email { get; set; } // Read-only purposes;

        public string PhoneNumber { get; set; } // Read-only purposes;

        public UserImage UserImage { get; set; }

        public ImageResizeInfo UserImageResize
        {
            get
            {
                return new ImageResizeInfo
                {
                    Format = ImageFormat.Jpg,
                    Width = 512,
                    Height = 512,
                    Mode = ImageMode.CropScale
                };
            }
        }
    }

    public class ProfileEditProfile : Profile
    {
        public ProfileEditProfile()
        {
            CreateMap<ProfileEditModel, User>()
                .ForMember(x => x.Email, x => x.Ignore())
                .ForMember(x => x.PhoneNumber, x => x.Ignore())
                .ForMember(x => x.UserImage, x => x.Ignore())
                .ForMember(x => x.UserImageId, x => x.Ignore());
            CreateMap<User, ProfileEditModel>();
        }
    }

    public class ProfileEditValidator : AbstractValidator<ProfileEditModel>
    {
        public ProfileEditValidator()
        {
            RuleFor(x => x.FirstName).NotNull().NotEmpty().Name();
            RuleFor(x => x.LastName).NotNull().NotEmpty().Name();
        }
    }
}
