using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Neimart.Core.Infrastructure.Sending
{
    public interface IEmailSender
    {
        Task SendAsync(string userName, string password, string displayName, string email, string subject, string body);

        Task SendAsync(string userName, string password, string displayName, string[] emails, string subject, string body);
    }
}
