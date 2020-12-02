using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using FluentValidation;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Options;
using Neimart.Core.Entities;
using Neimart.Core.Settings;

namespace Neimart.Web.Models
{
    public class OrderStatusModel
    {
        public long Id { get; set; }

        public OrderStatus Status { get; set; }

        public bool DeliveryRequired { get; set; }

        [Display(Name = "Processing information")]
        public string ProcessingInfo { get; set; }

        [Display(Name = "Delivery information")]
        public string DeliveryInfo { get; set; }

        public DateTimeOffset CancelledOn { get; set; }

        public string CancelReason { get; set; }

        [Display(Description = "Indicate whether to refund payment made for this order to the customer. Note that this process is irreversible.")]
        public bool Refunded { get; set; }
    }

    public class OrderStatusProfile : Profile
    {
        public OrderStatusProfile()
        {
            CreateMap<Order, OrderStatusModel>()
                .ForMember(x => x.Status, x => x.Ignore())
                .ForMember(x => x.DeliveryRequired, x => x.Ignore());
        }
    }

    public class OrderStatusValidator : AbstractValidator<OrderStatusModel>
    {
        public OrderStatusValidator()
        {
            RuleFor(x => x.ProcessingInfo)
                .Must((m, x) => m.Status == OrderStatus.Processing ? !string.IsNullOrWhiteSpace(x) : true)
                .WithMessage("'{PropertyName}' must not be empty.");

            RuleFor(x => x.DeliveryInfo)
                .Must((m, x) => m.Status == OrderStatus.Delivering ? !string.IsNullOrWhiteSpace(x) : true)
                .WithMessage("'{PropertyName}' must not be empty.");

            RuleFor(x => x.CancelReason).Must((m, x) => m.Status == OrderStatus.Cancelled ? !string.IsNullOrWhiteSpace(x) : true)
                .WithMessage("'{PropertyName}' must not be empty.");
        }
    }
}
