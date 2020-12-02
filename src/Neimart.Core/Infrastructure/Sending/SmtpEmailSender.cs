using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;

namespace Neimart.Core.Infrastructure.Sending
{
    public class SmtpEmailSender : IEmailSender
    {
        private readonly SmtpEmailSenderOptions _emailSenderOptions;

        public SmtpEmailSender(IOptions<SmtpEmailSenderOptions> emailSenderOptions)
        {
            _emailSenderOptions = emailSenderOptions.Value;
        }

        public async Task SendAsync(string userName, string password, string displayName, string[] emails, string subject, string body)
        {
            using (var smtpClient = new SmtpClient())
            {
                var message = new MailMessage
                {
                    Subject = subject,
                    Body = body,
                    IsBodyHtml = true,
                    From = new MailAddress(userName, displayName)
                };
                foreach (var email in emails) message.To.Add(email);

                smtpClient.Host = _emailSenderOptions.Server;
                smtpClient.Port = _emailSenderOptions.Port;
                smtpClient.Credentials = new NetworkCredential(userName, password, null);
                smtpClient.EnableSsl = _emailSenderOptions.EnableSsl;

                await smtpClient.SendMailAsync(message);
            }
        }

        public Task SendAsync(string userName, string password, string displayName, string email, string subject, string body)
        {
            return SendAsync(userName, password, displayName, new[] { email }, subject, body);
        }
    }
}