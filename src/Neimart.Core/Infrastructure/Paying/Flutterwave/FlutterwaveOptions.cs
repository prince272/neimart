using System;
using System.Collections.Generic;
using System.Text;

namespace Neimart.Core.Infrastructure.Paying.Flutterwave
{
    public class FlutterwaveOptions
    {
        public bool Live { get; set; }

        public string PublicKey { get; set; }

        public string SecretKey { get; set; }

        public string EncryptionKey { get; set; }
    }
}
