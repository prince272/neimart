using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Neimart.Core.Infrastructure.Paying
{
    public class PaymentIssuer
    {
        public string Name { get; set; }

        public string Code { get; set; }

        public PaymentMode Mode { get; set; }
    }
}
