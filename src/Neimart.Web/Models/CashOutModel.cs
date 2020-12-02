using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using FluentValidation;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Options;
using Neimart.Core.Entities;
using Neimart.Core.Infrastructure.Paying;
using Neimart.Core.Infrastructure.Web;
using Neimart.Core.Settings;

namespace Neimart.Web.Models
{
    public class CashOutModel
    {
        public string Reference { get; set; }

        public PaymentMode Mode { get; set; }

        [Display(Name = "Bank account number")]
        public string BankNumber { get; set; }

        [Display(Name = "Bank account issuer")]
        public string BankIssuer { get; set; }

        [Display(Name = "Mobile money issuer")]
        public string MobileIssuer { get; set; }

        public List<SelectListItem> MobileIssuerOptions { get; set; } = new List<SelectListItem>();

        public List<SelectListItem> BankIssuerOptions { get; set; } = new List<SelectListItem>();

        [Display(Name = "Mobile money number")]
        public string MobileNumber { get; set; }

        public string Issuer => Mode == PaymentMode.Bank ? BankIssuer :
                                Mode == PaymentMode.Mobile ? MobileIssuer :
                                throw new InvalidOperationException();
        public string AccountNumber => Mode == PaymentMode.Bank ? BankNumber :
                                       Mode == PaymentMode.Mobile ? MobileNumber :
                                       throw new InvalidOperationException();

        [Display(Name = "Payment fee")]
        public decimal Fee { get; set; }

        public decimal Amount { get; set; }

        public decimal TotalAmount => Fee + Amount;
    }

    public class CashOutValidator : AbstractValidator<CashOutModel>
    {
        public CashOutValidator(IOptions<AppSettings> appSettingsAccessor)
        {
            RuleFor(x => x.Reference).NotNull().NotEmpty();
            RuleFor(x => x.MobileNumber).NotNull().NotEmpty().Phone().When(x => x.Mode == PaymentMode.Mobile);
            RuleFor(x => x.BankNumber).NotNull().NotEmpty().When(x => x.Mode == PaymentMode.Bank);
            RuleFor(x => x.Amount).InclusiveBetween(appSettingsAccessor.Value.CurrencyMinValue, appSettingsAccessor.Value.CurrencyMaxValue);
        }
    }
}
