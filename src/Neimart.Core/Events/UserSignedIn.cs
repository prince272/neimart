using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using MediatR;
using Neimart.Core.Entities;

namespace Neimart.Core.Events
{
    public class UserSignedIn : INotification
    {
        private readonly User user;

        public UserSignedIn(User user)
        {
            this.user = user;
        }
    }
}
