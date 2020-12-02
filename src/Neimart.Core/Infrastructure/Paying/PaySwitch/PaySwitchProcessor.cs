using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
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

namespace Neimart.Core.Infrastructure.Paying.PaySwitch
{
    public class PaySwitchProcessor : IPaymentProcessor
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly PaySwitchOptions _paySwitchOptions;
        private readonly HttpClient _client;
        private readonly AppSettings _appSettings;
        private readonly ILogger<PaySwitchProcessor> _logger;

        public PaySwitchProcessor(IServiceProvider services)
        {
            _httpContextAccessor = services.GetRequiredService<IHttpContextAccessor>();
            _paySwitchOptions = services.GetRequiredService<IOptions<PaySwitchOptions>>().Value;
            _client = services.GetRequiredService<IHttpClientFactory>().CreateClient(nameof(PaySwitchProcessor));
            _client.BaseAddress = new Uri(_paySwitchOptions.Live ? "https://prod.theteller.net" : "https://test.theteller.net");
            _appSettings = services.GetRequiredService<IOptions<AppSettings>>().Value;
            _logger = services.GetRequiredService<ILogger<PaySwitchProcessor>>();
        }

        public string Name => "PaySwitch";

        public async Task ProcessAsync(Transaction transaction)
        {
            if (transaction == null)
                throw new ArgumentNullException(nameof(transaction));
            
            Dictionary<string, string> headers, data;

            if (transaction.Type != TransactionType.Withdrawal)
            {
                // Request Headers
                headers = new Dictionary<string, string>();
                headers.Add("Cache-Control", "no-cache");
                headers.Add("Authorization", transaction.AuthorizationCode);

                // Request Data
                data = new Dictionary<string, string>();
                data.Add("merchant_id", _paySwitchOptions.MerchantId);
                data.Add("transaction_id", transaction.TransactionCode);
                data.Add("desc", transaction.Description);
                data.Add("amount", ((transaction.Amount + transaction.Fee) * 100).ToString("000000000000"));
                data.Add("redirect_url", transaction.CallbackUrl);
                data.Add("email", transaction.AccountEmail);

                _logger.LogInformation("Request: " + ReadAsString(data));
                var response = await _client.SendAsJsonAsync("/checkout/initiate", HttpMethod.Post, data: data, headers: headers);
                var result = await response.Content.ReadAsJsonAsync<Dictionary<string, string>>();
                _logger.LogInformation("Response: " + ReadAsString(result));

                var success = result.GetValueOrDefault("code") == "200";
                transaction.Status = success ? TransactionStatus.Pending : TransactionStatus.Failed;
                transaction.Message = result.GetValueOrDefault("reason");
                transaction.CheckoutUrl = result.GetValueOrDefault("checkout_url");
            }
            else
            {
                // Request Headers
                headers = new Dictionary<string, string>();
                headers.Add("Cache-Control", "no-cache");
                headers.Add("Authorization", transaction.AuthorizationCode);

                // Request Data
                data = new Dictionary<string, string>();
                data.Add("merchant_id", _paySwitchOptions.MerchantId);
                data.Add("transaction_id", transaction.TransactionCode);
                data.Add("amount", ((transaction.Amount) * 100).ToString("000000000000"));
                data.Add("processing_code", transaction.Mode == PaymentMode.Mobile ? "404000" :
                                            transaction.Mode == PaymentMode.Bank ? "404020" : 
                                            throw new InvalidOperationException());
                data.Add("r-switch", "FLT");
                data.Add("desc", transaction.Description);
                data.Add("pass_code", _paySwitchOptions.Passcode);
                data.Add("account_number", transaction.AccountNumber);
                data.Add("account_issuer", transaction.Mode == PaymentMode.Bank ? "GIP" : transaction.Issuer.ToString());
                if (transaction.Mode == PaymentMode.Bank) data.Add("account_bank", transaction.Issuer.ToString());

                _logger.LogInformation("Request: " + ReadAsString(data));
                var response = await _client.SendAsJsonAsync("/v1.1/transaction/process", HttpMethod.Post, data: data, headers: headers);
                var result = await response.Content.ReadAsJsonAsync<Dictionary<string, string>>();
                _logger.LogInformation("Response: " + ReadAsString(result));

                var success = result.GetValueOrDefault("code") == "000";
                // Mobile transactions for withdrawal is only issued once. So return succeeded if things went well.
                transaction.Status = success ? transaction.Mode == PaymentMode.Bank ? 
                    TransactionStatus.Pending : 
                    TransactionStatus.Succeeded : 
                    TransactionStatus.Failed;

                transaction.Message = result.GetValueOrDefault("reason");
                transaction.AccountName = result.GetValueOrDefault("account_name");
            }
        }

        public async Task VerifyAsync(Transaction transaction)
        {
            if (transaction == null)
                throw new ArgumentNullException(nameof(transaction));

            Dictionary<string, string> headers, data;

            if (transaction.Type != TransactionType.Withdrawal)
            {  
                // Request Headers
                headers = new Dictionary<string, string>();
                headers.Add("Cache-Control", "no-cache");
                headers.Add("Merchant-Id", _paySwitchOptions.MerchantId);

                // Request Data
                data = new Dictionary<string, string>();

                _logger.LogInformation("Request: " + ReadAsString(data));
                var response = await _client.SendAsJsonAsync($"/v1.1/users/transactions/{transaction.TransactionCode}/status", HttpMethod.Get, data: data, headers: headers);
                var result = await response.Content.ReadAsJsonAsync<Dictionary<string, string>>();
                _logger.LogInformation("Response: " + ReadAsString(result));

                if (transaction.Status == TransactionStatus.Pending)
                {
                    var transactionStatus = result.GetValueOrDefault("code") == "000"
                        ? TransactionStatus.Succeeded : TransactionStatus.Failed;
                    var transactionMessage = result.GetValueOrDefault("reason");
                    var paymentIssuer = result.GetValueOrDefault("r_switch");

                    var issuers = await GetIssuersAsync();
                    var paymentMode = issuers.FirstOrDefault(x => x.Code == paymentIssuer)?.Mode;

                    transaction.Status = transactionStatus;
                    transaction.Message = transactionMessage;
                    transaction.Issuer = paymentIssuer;
                    transaction.Mode = paymentMode;
                }
            }
            else
            {
                // Request Headers
                headers = new Dictionary<string, string>();
                headers.Add("Cache-Control", "no-cache");
                headers.Add("Authorization", transaction.AuthorizationCode);

                // Request Data
                data = new Dictionary<string, string>();
                data.Add("merchant_id", _paySwitchOptions.MerchantId);
                data.Add("reference_id", transaction.TransactionCode);

                _logger.LogInformation("Request: " + ReadAsString(data));
                var response = await _client.SendAsJsonAsync($"/v1.1/transaction/bank/ftc/authorize", HttpMethod.Post, data: data, headers: headers);
                var result = await response.Content.ReadAsJsonAsync<Dictionary<string, string>>();
                _logger.LogInformation("Response: " + ReadAsString(result));
            }
        }

        public Task<IEnumerable<PaymentIssuer>> GetIssuersAsync()
        {
            var list = new List<PaymentIssuer>();

            list.Add(new PaymentIssuer { Name = "MTN", Code = "MTN", Mode = PaymentMode.Mobile });
            list.Add(new PaymentIssuer { Name = "Vodafone", Code = "VDF", Mode = PaymentMode.Mobile });
            list.Add(new PaymentIssuer { Name = "Airtel", Code = "ATL", Mode = PaymentMode.Mobile });
            list.Add(new PaymentIssuer { Name = "Tigo", Code = "TGO", Mode = PaymentMode.Mobile });

            list.Add(new PaymentIssuer { Name = "Visa Card", Code = "VIS", Mode = PaymentMode.Card });
            list.Add(new PaymentIssuer { Name = "Master Card", Code = "MAS", Mode = PaymentMode.Card });

            list.Add(new PaymentIssuer { Name = "Float Account", Code = "FLT", Mode = PaymentMode.Virtual });
            list.Add(new PaymentIssuer { Name = "User Account", Code = "USE", Mode = PaymentMode.Virtual });
            list.Add(new PaymentIssuer { Name = "Standard Chartered Bank", Code = "SCH", Mode = PaymentMode.Bank });
            list.Add(new PaymentIssuer { Name = "Absa Bank Ghana Limited", Code = "ABG", Mode = PaymentMode.Bank });
            list.Add(new PaymentIssuer { Name = "Gcb Bank Limited", Code = "GCB", Mode = PaymentMode.Bank });
            list.Add(new PaymentIssuer { Name = "National Investment Bank", Code = "NIB", Mode = PaymentMode.Bank });
            list.Add(new PaymentIssuer { Name = "Agricultural Development Bank", Code = "ADB", Mode = PaymentMode.Bank });
            list.Add(new PaymentIssuer { Name = "Universal Merchant Bank", Code = "UMB", Mode = PaymentMode.Bank });
            list.Add(new PaymentIssuer { Name = "Republic Bank Limited", Code = "RBL", Mode = PaymentMode.Bank });
            list.Add(new PaymentIssuer { Name = "Zenith Bank Ghana Ltd", Code = "ZEN", Mode = PaymentMode.Bank });
            list.Add(new PaymentIssuer { Name = "Ecobank Ghana Ltd", Code = "ECO", Mode = PaymentMode.Bank });
            list.Add(new PaymentIssuer { Name = "Cal Bank Limited", Code = "CAL", Mode = PaymentMode.Bank });
            list.Add(new PaymentIssuer { Name = "Prudential Bank Ltd", Code = "PRD", Mode = PaymentMode.Bank });
            list.Add(new PaymentIssuer { Name = "Stanbic Bank", Code = "STB", Mode = PaymentMode.Bank });
            list.Add(new PaymentIssuer { Name = "Guaranty Trust Bank", Code = "GTB", Mode = PaymentMode.Bank });
            list.Add(new PaymentIssuer { Name = "United Bank Of Africa", Code = "UBA", Mode = PaymentMode.Bank });
            list.Add(new PaymentIssuer { Name = "Access Bank Ltd", Code = "ACB", Mode = PaymentMode.Bank });
            list.Add(new PaymentIssuer { Name = "Consolidated Bank  Ghana", Code = "CBG", Mode = PaymentMode.Bank });
            list.Add(new PaymentIssuer { Name = "First National Bank", Code = "FNB", Mode = PaymentMode.Bank });
            list.Add(new PaymentIssuer { Name = "Unity Link", Code = "UNL", Mode = PaymentMode.Bank });
            list.Add(new PaymentIssuer { Name = "Fidelity Bank Limited", Code = "FDL", Mode = PaymentMode.Bank });
            list.Add(new PaymentIssuer { Name = "Services Integrity Savings & Loans", Code = "SIS", Mode = PaymentMode.Bank });
            list.Add(new PaymentIssuer { Name = "Bank Of Africa", Code = "BOA", Mode = PaymentMode.Bank });
            list.Add(new PaymentIssuer { Name = "Dalex Finance And Leasing Company", Code = "DFL", Mode = PaymentMode.Bank });
            list.Add(new PaymentIssuer { Name = "First Bank Of Nigeria", Code = "FBO", Mode = PaymentMode.Bank });
            list.Add(new PaymentIssuer { Name = "Ghl Bank", Code = "GHL", Mode = PaymentMode.Bank });
            list.Add(new PaymentIssuer { Name = "Bank Of Ghana", Code = "BOG", Mode = PaymentMode.Bank });
            list.Add(new PaymentIssuer { Name = "First Atlantic Bank", Code = "FAB", Mode = PaymentMode.Bank });
            list.Add(new PaymentIssuer { Name = "Sahel - Sahara Bank (Bsic)", Code = "SSB", Mode = PaymentMode.Bank });
            list.Add(new PaymentIssuer { Name = "G-Money", Code = "GMY", Mode = PaymentMode.Bank });
            list.Add(new PaymentIssuer { Name = "Arb  Apex Bank Limited", Code = "APX", Mode = PaymentMode.Bank });

            return Task.FromResult(list.AsEnumerable());
        }

        public ValueTask<string> GetAuthorizationCodeAsync()
        {
            var authorizationCode = $"Basic {Convert.ToBase64String(Encoding.UTF8.GetBytes($"{_paySwitchOptions.ApiUser}:{_paySwitchOptions.ApiKey}"))}";
            return new ValueTask<string>(authorizationCode);
        }

        public ValueTask<string> GenerateTransactionCodeAsync()
        {
            var transactionCode = ComputeHelper.GenerateRandomString(12, ComputeHelper.NaturalNumericChars);
            return new ValueTask<string>(transactionCode);
        }

        public ValueTask<string> GetTransactionCodeAsync()
        {
            string transactionCode = _httpContextAccessor.HttpContext.Request.Query["transaction_id"].ToString();
            return new ValueTask<string>(transactionCode);
        }

        private string ReadAsString(IDictionary<string, string> dic)
        {
            return string.Join(";", dic.Select(x => x.Key + "=" + x.Value).ToArray());
        }
    }
}