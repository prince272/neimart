using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using FluentValidation;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.DependencyInjection;
using Neimart.Core.Entities;
using Neimart.Core.Filters;
using Neimart.Core.Infrastructure.Web;
using Neimart.Core.Services;
using Neimart.Core.Utilities.Helpers;

namespace Neimart.Web.Models
{
    public class AddressEditModel
    {
        public bool IsEdit => Id != 0;

        public long Id { get; set; }

        public string FirstName { get; set; }

        public string LastName { get; set; }

        [Display(Name = "Phone")]
        public string PhoneNumber { get; set; }

        public string Organization { get; set; }

        public string Region { get; set; }

        public string Place { get; set; }

        [Display(Name = "Street, apartment, suite, unit")]
        public string Street { get; set; }

        [Display(Name = "Digital/Postal address")]
        public string Postal { get; set; }

        public string Email { get; set; }

        public List<AddressType> AddressTypes { get; } = new List<AddressType>();
    }

    public class AddressEditProfile : Profile
    {
        public AddressEditProfile()
        {
            CreateMap<AddressEditModel, Address>();
            CreateMap<Address, AddressEditModel>();
        }
    }

    public class AddressEditValidator : AbstractValidator<AddressEditModel>
    {
        public AddressEditValidator(IServiceProvider services)
        {
            RuleFor(x => x.FirstName).NotNull().NotEmpty();
            RuleFor(x => x.LastName).NotNull().NotEmpty();
            RuleFor(x => x.Email).NotNull().NotEmpty().Email();
            RuleFor(x => x.PhoneNumber).NotNull().NotEmpty().Phone();

            RuleFor(x => x.Region).NotNull().NotEmpty();
            RuleFor(x => x.Place).NotNull().NotEmpty();
            RuleFor(x => x.Street).NotNull().NotEmpty();
        }
    }
}
