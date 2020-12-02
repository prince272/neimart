using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.AspNetCore.Identity;

namespace Neimart.Core.Entities
{
    public class UserRole : IdentityUserRole<long>, IEntity
    {
        long IEntity.Id { get; set; }

        public virtual User User { get; set; }

        public virtual Role Role { get; set; }
    }
}
