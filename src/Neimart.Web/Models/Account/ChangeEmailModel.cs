using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using FluentValidation;
using Neimart.Core.Infrastructure.Web;

namespace Neimart.Web.Models.Account
{
    public class ChangeEmailModel
    {
        public string OldEmail { get; set; }

        public string NewEmail { get; set; }
    }

    public class ChangeEmailValidator : AbstractValidator<ChangeEmailModel>
    {
        public ChangeEmailValidator()
        {
            RuleFor(x => x.OldEmail).NotNull().NotEmpty();
            RuleFor(x => x.NewEmail).NotNull().NotEmpty().Email().NotEqual(x => x.OldEmail);
        }
    }
}
