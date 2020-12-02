using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using FluentValidation;
using Neimart.Core.Infrastructure.Web;

namespace Neimart.Web.Models.Account
{
    public class ResetPasswordModel
    {
        public string Password { get; set; }

        public string ConfirmPassword { get; set; }
    }

    public class ResetPasswordValidator : AbstractValidator<ResetPasswordModel>
    {
        public ResetPasswordValidator()
        {
            RuleFor(x => x.Password).NotNull().NotEmpty().Password();
            RuleFor(x => x.ConfirmPassword).NotNull().NotEmpty().Equal(x => x.Password);
        }
    }
}
