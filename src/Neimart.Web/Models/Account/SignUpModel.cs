using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using FluentValidation;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;
using Neimart.Core.Infrastructure.Web;

namespace Neimart.Web.Models.Account
{
    public class SignUpModel
    {
        public string FirstName { get; set; }

        public string LastName { get; set; }

        public string Email { get; set; }

        public string PhoneNumber { get; set; }

        public string Password { get; set; }

        public bool IsAgreed { get; set; }

        public List<AuthenticationScheme> ExternalLogins { get; } = new List<AuthenticationScheme>();
    }

    public class SignUpValidator : AbstractValidator<SignUpModel>
    {
        public SignUpValidator()
        {
            RuleFor(x => x.FirstName).NotNull().NotEmpty().Name();
            RuleFor(x => x.LastName).NotNull().NotEmpty().Name();

            RuleFor(x => x.Email).NotNull().NotEmpty().Email();

            RuleFor(x => x.Password).NotNull().NotEmpty().Password();

            RuleFor(x => x.PhoneNumber).NotNull().NotEmpty().Phone();

            RuleFor(x => x.IsAgreed).Equal(true).WithMessage("Please agree to create an account.");
        }
    }
}
