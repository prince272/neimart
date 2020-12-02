using System;
using System.Collections.Generic;
using System.Text;

namespace Neimart.Core.Infrastructure.Paying.PaySwitch
{
    public class PaySwitchOptions
    {
        public string MerchantId { get; set; }

        public string ApiKey { get; set; }

        public string ApiUser { get; set; }

        public bool Live { get; set; }

        public string Passcode { get; set; }
    }
}