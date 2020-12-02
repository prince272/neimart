using System;
using System.Collections.Generic;
using System.Text;
using MediatR;
using Neimart.Core.Entities;

namespace Neimart.Core
{
    public interface IEntityNotification<TEntity> : INotification
        where TEntity : class, IEntity
    {
        public User Seller { get; set; }

        public TEntity Entity { get; set; }
    }
}
