using System;
using System.Collections.Generic;
using System.Text;
using MediatR;
using Neimart.Core.Entities;

namespace Neimart.Core.Events
{
    public class EntityUpdated<TEntity> : IEntityNotification<TEntity>
        where TEntity : class, IEntity
    {
        public EntityUpdated(TEntity entity, User seller = null)
        {
            Entity = entity;
            Seller = seller;
        }

        public TEntity Entity { get; set; }

        public User Seller { get; set; }
    }
}
