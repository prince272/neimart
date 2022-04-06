using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using MimeKit;
using System;
using System.Threading.Tasks;

namespace Neimart.Core.Infrastructure.Sending
{
    public class SmtpEmailSender : IEmailSender
    {
        private readonly SmtpEmailSenderOptions _emailSenderOptions;

        public SmtpEmailSender(IOptions<SmtpEmailSenderOptions> emailSenderOptions)
        {
            _emailSenderOptions = emailSenderOptions.Value;
        }

        public async Task SendAsync(string userName, string password, string displayName, string email, string subject, string body)
        {
            var message = new MimeMessage();

            message.Subject = subject;
            message.From.Add(new MailboxAddress(displayName, userName));
            message.To.Add(new MailboxAddress(string.Empty, email));

            var builder = new BodyBuilder();
            builder.HtmlBody = body;

            message.Body = builder.ToMessageBody();

            using (var smtpClient = new SmtpClient())
            {
                smtpClient.ServerCertificateValidationCallback = (s, c, h, e) => true;
                await smtpClient.ConnectAsync(_emailSenderOptions.Server, _emailSenderOptions.Port, (SecureSocketOptions)2);
                await smtpClient.AuthenticateAsync(userName, password);
                await smtpClient.SendAsync(message);
                await smtpClient.DisconnectAsync(true);
            }
        }
    }
}