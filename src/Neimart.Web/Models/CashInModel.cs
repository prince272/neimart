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
    public class CashInModel
    {
        [Display(Name = "Payment processor")]
        public TransactionProcessor Processor { get; set; }

        public List<SelectListItem> ProcessorOptions { get; set; } = new List<SelectListItem>();

        public TransactionType Type { get; set; }

        [Display(Name = "Payment fee")]
        public decimal Fee { get; set; }

        public decimal Amount { get; set; }

        public decimal TotalAmount => Fee + Amount;

        public string Reference { get; set; }

        public bool ProcessorHide { get; set; }
    }

    public class TransactionInValidator : AbstractValidator<CashInModel>
    {
        public TransactionInValidator(IOptions<AppSettings> appSettingsAccessor)
        {
            RuleFor(x => x.Type).NotEqual(TransactionType.Withdrawal);
            RuleFor(x => x.Processor).Equal(TransactionProcessor.External).When(x => x.Type == TransactionType.Deposit || x.Type == TransactionType.Withdrawal || x.Type == TransactionType.Subscription);
            RuleFor(x => x.Reference).NotNull().NotEmpty();
            RuleFor(x => x.Amount).InclusiveBetween(appSettingsAccessor.Value.CurrencyMinValue, appSettingsAccessor.Value.CurrencyMaxValue).When(x => x.Type == TransactionType.Deposit);
        }
    }
}
