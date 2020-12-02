using System;
using System.Collections.Generic;
using System.Text;
using MediatR;
using Neimart.Core.Entities;

namespace Neimart.Core.Events
{
    public class EntityViewed<TEntity> : IEntityNotification<TEntity>
        where TEntity : class, IEntity
    {
        public EntityViewed(TEntity entity, User seller = null)
        {
            Entity = entity;
            Seller = seller;
        }

        public TEntity Entity { get; set; }

        public User Seller { get; set; }
    }
}
