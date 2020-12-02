using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using FluentValidation;
using Neimart.Core.Infrastructure.Web;

namespace Neimart.Web.Models.Account
{
    public class ForgotPasswordModel
    {
        public string Email { get; set; }
    }

    public class ForgotPasswordValidator : AbstractValidator<ForgotPasswordModel>
    {
        public ForgotPasswordValidator()
        {
            RuleFor(x => x.Email).NotNull().NotEmpty().Email();
        }
    }
}
