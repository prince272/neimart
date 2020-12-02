using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using FluentValidation;
using Microsoft.Extensions.Options;
using Neimart.Core.Infrastructure.Web;

namespace Neimart.Web.Models.Account
{
    public class ChangePhoneNumberModel
    {
        public string NewPhoneNumber { get; set; }
    }

    public class ChangePhoneNumberValidator : AbstractValidator<ChangePhoneNumberModel>
    {
        public ChangePhoneNumberValidator()
        {
            RuleFor(x => x.NewPhoneNumber).NotNull().NotEmpty().Phone();
        }
    }
}
