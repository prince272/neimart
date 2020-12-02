using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentValidation;
using Microsoft.AspNetCore.Mvc.Rendering;
using Neimart.Core.Entities;
using Neimart.Core.Infrastructure.Web;

namespace Neimart.Web.Models
{
    public class CheckoutModel
    {
        public CartListModel CartListModel { get; set; }

        public List<AddressModel> AddressModels { get; set; } = new List<AddressModel>();

        public decimal DeliveryFee { get; set; }

        public bool DeliveryCalculated { get; set; }

        public bool DeliveryRequired { get; set; }

        public decimal SubtotalAmount { get; set; }

        public decimal TotalAmount { get; set; }
    }

    public class CheckoutValidator : AbstractValidator<CheckoutModel>
    {
        public CheckoutValidator()
        {
        }
    }
}
