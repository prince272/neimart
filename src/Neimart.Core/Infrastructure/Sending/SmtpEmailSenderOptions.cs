using System;
using System.Collections.Generic;
using System.Text;

namespace Neimart.Core.Infrastructure.Sending
{
    public class SmtpEmailSenderOptions
    {
        public string Server { get; set; }

        public int Port { get; set; }

        public bool EnableSsl { get; set; }
    }
}
