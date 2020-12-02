using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentValidation;
using Neimart.Core.Infrastructure.Web;

namespace Neimart.Web.Models
{
    public class ContactModel
    {
        public string Name { get; set; }

        public string Subject { get; set; }

        public string Message { get; set; }

        public string Email { get; set; }

        public string PhoneNumber { get; set; }
    }

    public class ContactValidator : AbstractValidator<ContactModel>
    {
        public ContactValidator()
        {
            RuleFor(x => x.Name).NotNull().NotEmpty();
            RuleFor(x => x.Subject).NotNull().NotEmpty();
            RuleFor(x => x.Message).NotNull().NotEmpty();
            RuleFor(x => x.Email).NotNull().NotEmpty().Email();
        }
    }
}
