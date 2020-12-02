using System;
using System.Collections.Generic;
using System.Text;
using Neimart.Core.Entities;
using Neimart.Core.Infrastructure.Paying;

namespace Neimart.Core.Filters
{
    public class TransactionFilter
    {
        public long? MemberId { get; set; }

        public long? TransactionId { get; set; }

        public TransactionStatus? Status { get; set; }

        public TransactionProcessor? Processor { get; set; }

        public PaymentMode? Mode { get; set; }

        public TransactionType? Type { get; set; }

        public string TransactionCode { get; set; }

        public string Search { get; set; }
    }
}