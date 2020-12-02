using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Neimart.Core.Entities;
using Neimart.Core.Filters;
using Neimart.Core.Infrastructure.Database;
using Neimart.Core.Infrastructure.Web;
using Neimart.Core.Services;
using Neimart.Core.Settings;
using Neimart.Core.Utilities.Extensions;
using Neimart.Core.Utilities.Helpers;

namespace Neimart.Core.Infrastructure.Paying.Flutterwave
{
    public class FlutterwaveProcessor : IPaymentProcessor
    {
        private readonly HttpClient _client;
        private readonly FlutterwaveOptions _flutterwaveOptions;
        private readonly AppSettings _appSettings;
        public FlutterwaveProcessor(IServiceProvider services)
        {
            _client = services.GetRequiredService<IHttpClientFactory>().CreateClient(nameof(FlutterwaveProcessor));
            _client.BaseAddress = new Uri("https://api.flutterwave.com");
            _flutterwaveOptions = services.GetRequiredService<IOptions<FlutterwaveOptions>>().Value;
            _appSettings = services.GetRequiredService<IOptions<AppSettings>>().Value;
        }

        public string Name => "Flutterwave";

        public ValueTask<string> GetAuthorizationCodeAsync()
        {
            var authorizationCode = $"Bearer {_flutterwaveOptions.SecretKey}";
            return new ValueTask<string>(authorizationCode);
        }

        public ValueTask<string> GenerateTransactionCodeAsync()
        {
            var transactionCode = ComputeHelper.GenerateRandomString(12, ComputeHelper.NaturalNumericChars);
            return new ValueTask<string>(transactionCode);
        }

        public ValueTask<string> GetTransactionCodeAsync()
        {
            string transactionCode = null;
            return new ValueTask<string>(transactionCode);
        }

        public async Task ProcessAsync(Transaction transaction)
        {
            if (transaction == null)
                throw new ArgumentNullException(nameof(transaction));

            if (transaction.Type != TransactionType.Withdrawal)
            {
                var headers = new Dictionary<string, string>();
                headers.Add("Authorization", transaction.AuthorizationCode);

                var data = new
                {
                    tx_ref = transaction.TransactionCode,
                    amount = transaction.Amount,
                    currency = _appSettings.CurrencyCode,
                    redirect_url = transaction.RedirectUrl,
                    customer = new
                    {
                        email = transaction.AccountEmail,
                        phonenumber = transaction.AccountNumber,
                        name = transaction.AccountName
                    },
                    customizations = new
                    {
                        title = transaction.Title,
                        description = transaction.Description,
                        logo = transaction.Logo
                    }
                };

                var response = await _client.SendAsJsonAsync("/v3/payments", HttpMethod.Post, data: data, headers: headers);
                var result = await response.Content.ReadAsJsonAsync<dynamic>();

                var success = result.status == "success";
                transaction.Status = success ? TransactionStatus.Pending : TransactionStatus.Failed;
                transaction.Message = result.message;

                if (success)
                {
                    transaction.CheckoutUrl = result.data.link;
                }
            }
            else
            {

            }
        }

        public Task VerifyAsync(Transaction transaction)
        {
            if (transaction == null)
                throw new ArgumentNullException(nameof(transaction));

            return Task.CompletedTask;
        }

        public Task<IEnumerable<PaymentIssuer>> GetIssuersAsync()
        {
            var list = new List<PaymentIssuer>();
            return Task.FromResult(list.AsEnumerable());
        }
    }
}