using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Neimart.Core.Entities;

namespace Neimart.Core.Infrastructure.Paying
{
    public interface IPaymentProcessor
    {
        string Name { get; }

        Task ProcessAsync(Transaction transaction);

        Task VerifyAsync(Transaction transaction);

        Task<IEnumerable<PaymentIssuer>> GetIssuersAsync();

        ValueTask<string> GetTransactionCodeAsync();

        ValueTask<string> GenerateTransactionCodeAsync();

        ValueTask<string> GetAuthorizationCodeAsync();
    }
}
