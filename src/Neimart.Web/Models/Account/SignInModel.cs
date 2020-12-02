using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using FluentValidation;
using Microsoft.AspNetCore.Authentication;
using Neimart.Core.Infrastructure.Web;

namespace Neimart.Web.Models.Account
{
    public class SignInModel
    {
        public string Email { get; set; }

        public string Password { get; set; }

        [Display(Description = "Stay signed into account")]
        public bool IsPersistent { get; set; }

        public List<AuthenticationScheme> ExternalLogins { get; } = new List<AuthenticationScheme>();
    }

    public class SignInValidator : AbstractValidator<SignInModel>
    {
        public SignInValidator()
        {
            RuleFor(x => x.Email).NotNull().NotEmpty().Email();
            RuleFor(x => x.Password).NotNull().NotEmpty();
        }
    }
}
