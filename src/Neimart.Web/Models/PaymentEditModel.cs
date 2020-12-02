using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Rendering;
using Neimart.Core.Infrastructure.Paying;

namespace Neimart.Web.Models
{
    public class PaymentEditModel
    {
        public List<SelectListItem> BankIssuerOptions { get; set; } = new List<SelectListItem>();
 
        [Display(Name = "Bank account issuer")]
        public string BankIssuer { get; set; }

        [Display(Name = "Bank account number")]
        public string BankNumber { get; set; }

        [Display(Name = "Mobile money issuer")]
        public string MobileIssuer { get; set; }

        [Display(Name = "Mobile money number")]
        public string MobileNumber { get; set; }

        public List<SelectListItem> MobileIssuerOptions { get; set; } = new List<SelectListItem>();

        public PaymentMode Mode { get; set; }
    }
}
