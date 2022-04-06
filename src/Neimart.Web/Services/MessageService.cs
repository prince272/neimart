using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Neimart.Core;
using Neimart.Core.Entities;
using Neimart.Core.Infrastructure.Sending;
using Neimart.Core.Infrastructure.Web;
using Neimart.Core.Settings;
using Neimart.Core.Utilities.Extensions;
using Neimart.Core.Utilities.Helpers;
using Neimart.Web.Models;

namespace Neimart.Web.Services
{
    public class MessageService
    {
        private readonly IEmailSender _emailSender;
        private readonly ISmsSender _smsSender;
        private readonly IRazorViewRenderer _razorViewRenderer;
        private readonly IOptions<AppSettings> _appSettings;
        private readonly ILogger<MessageService> _logger;

        public MessageService(IEmailSender emailSender, ISmsSender smsSender, IRazorViewRenderer razorViewRenderer, IOptions<AppSettings> appSettings, ILogger<MessageService> logger)
        {
            _emailSender = emailSender;
            _smsSender = smsSender;
            _razorViewRenderer = razorViewRenderer;
            _appSettings = appSettings;
            _logger = logger;
        }

        public Task SendEmailAsync(MessageRole messageRole, string messageDisplay, string email, string subject, string body)
        {
            var (username, password) = GetRoleCredentials(messageRole);

            Task.Run(() => _emailSender.SendAsync(username, password, messageDisplay, email, subject, body)).Forget();

            return Task.CompletedTask;
        }

        public Task SendEmailAsync<TModel>(MessageRole messageRole, MessageType messageType, string messageDisplay, string email, TModel model)
        {
            string subject = messageType.GetEnumText();
            return SendEmailAsync(messageRole, messageType, messageDisplay, email, subject, model);
        }

        public async Task SendEmailAsync<TModel>(MessageRole messageRole, MessageType messageType, string messageDisplay, string email, string subject, TModel model)
        {
            string body = await _razorViewRenderer.RenderViewToStringAsync($"~/Views/Shared/Templates/Email/{messageType}.cshtml", model);
            await SendEmailAsync(messageRole, messageDisplay, email, subject, body);
        }

        private (string userName, string password) GetRoleCredentials(MessageRole role)
        {
            return role switch
            {
                MessageRole.Admin => (_appSettings.Value.Company.AdminEmail, _appSettings.Value.Company.AdminPassword),
                MessageRole.Notification => (_appSettings.Value.Company.NotificationEmail, _appSettings.Value.Company.NotificationPassword),
                MessageRole.Info => (_appSettings.Value.Company.InfoEmail, _appSettings.Value.Company.InfoPassword),
                _ => throw new InvalidOperationException(),
            };
        }

        public Task SendCustomerOrderEmailAsync(OrderModel orderModel)
        {
            string subject = orderModel.Order.Status == OrderStatus.Pending ? "Your order has been placed" :
                             orderModel.Order.Status == OrderStatus.Processing ? "Your order is processing" :
                             orderModel.Order.Status == OrderStatus.Delivering ? "Your order is on the way" :
                             orderModel.Order.Status == OrderStatus.Complete ? "Your order has been complete" :
                             orderModel.Order.Status == OrderStatus.Cancelled ? "Your order has been cancelled" :
                             throw new InvalidOperationException();

            return SendEmailAsync(
                                    messageRole: MessageRole.Notification,
                                    messageType: MessageType.CustomerOrder,
                                    messageDisplay: $"{orderModel.Order.Seller.StoreName} via Neimart",
                                    email: orderModel.Order.Customer.Email,
                                    subject: subject,
                                    model: new ValueTuple<string, OrderModel>(subject, orderModel));
        }

        public Task SendStoreOrderEmailAsync(OrderModel orderModel)
        {
            string subject = "Nice! You've just got an order";

            return SendEmailAsync(
                                    messageRole: MessageRole.Notification,
                                    messageType: MessageType.StoreOrder,
                                    messageDisplay: $"{orderModel.Order.Seller.StoreName} via Neimart",
                                    email: orderModel.Order.Seller.Email,
                                    subject: subject,
                                    model: new ValueTuple<string, OrderModel>(subject, orderModel));
        }
    }

    public enum MessageType
    {
        [Display(Name = "Change your email address")]
        ChangeEmail,

        [Display(Name = "Verify your email address")]
        VerifyEmail,

        [Display(Name = "Reset your password")]
        ResetPassword,

        CustomerOrder,
        StoreOrder,
        StoreContact,

        CompanyContact,
        CompanyWelcome
    }

    public enum MessageRole
    {
        Admin,
        Info,
        Notification
    }
}
