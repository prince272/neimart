using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Neimart.Core.Infrastructure.Paying
{
    public enum PaymentMode
    {
        [Display(Name = "Mobile Money")]
        Mobile,
        [Display(Name = "Card Payment")]
        Card,
        [Display(Name = "Virtual Money")]
        Virtual,
        [Display(Name = "Bank Transfer")]
        Bank,
    }
}
