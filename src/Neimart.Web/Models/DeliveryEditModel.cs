using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Neimart.Core.Entities;
using Neimart.Core.Settings;

namespace Neimart.Web.Models
{
    public class DeliveryEditModel
    {
        public string Region { get; set; }

        public string Place { get; set; }

        public decimal Fee { get; set; }
    }
    public class DeliveryEditProfile : Profile
    {
        public DeliveryEditProfile()
        {
            CreateMap<DeliveryEditModel, Delivery>();
            CreateMap<Delivery, DeliveryEditModel>();
        }
    }
    public class DeliveryEditValidator : AbstractValidator<DeliveryEditModel>
    {
        public DeliveryEditValidator(IServiceProvider services)
        {
            var appSettingsAccessor = services.GetRequiredService<IOptions<AppSettings>>();
            RuleFor(x => x.Fee).NotNull().InclusiveBetween(0, appSettingsAccessor.Value.CurrencyMaxValue);
        }
    }
}
