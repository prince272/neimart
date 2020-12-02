using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using Neimart.Core.Infrastructure.Paying;
using Neimart.Core.Utilities.Extensions;
using Neimart.Core.Utilities.Helpers;

namespace Neimart.Core.Entities
{
    public class Transaction : IEntity, IExtendable
    {
        public virtual User Member { get; set; }

        public long MemberId { get; set; }

        public long Id { get; set; }

        public string AccountName { get; set; }

        public string AccountNumber { get; set; }

        public string AccountEmail { get; set; }

        public decimal AccountBalance { get; set; }

        public decimal Amount { get; set; }

        public decimal Fee { get; set; }

        public string Title { get; set; }

        public string Description { get; set; }

        public string Logo { get; set; }

        public TransactionProcessor Processor { get; set; }

        public TransactionStatus Status { get; set; }

        public TransactionType Type { get; set; }

        public string Issuer { get; set; }

        public PaymentMode? Mode { get; set; }

        [NotMapped]
        public UAParser.ClientInfo ClientInfo => UAParser.Parser.GetDefault().Parse(UserAgent ?? string.Empty);

        public string Message { get; set; }

        public DateTimeOffset CreatedOn { get; set; }

        public DateTimeOffset UpdatedOn { get; set; }

        public bool Updated { get; set; }

        public string RedirectUrl { get; set; }

        public string CallbackUrl { get; set; }

        public string CheckoutUrl { get; set; }

        public string TransactionCode { get; set; }

        public string AuthorizationCode { get; set; }

        public string Reference { get; set; }

        public string IpAddress { get; set; }

        public string UserAgent { get; set; }

       public string ExtensionData { get; set; }
    }

    public enum TransactionStatus
    {
        Pending,
        Failed,
        Succeeded
    }

    public enum TransactionType
    {
        Deposit,
        Withdrawal,
        Order,
        Subscription
    }

    public enum TransactionProcessor
    {
        External,
        Internal
    }
}
