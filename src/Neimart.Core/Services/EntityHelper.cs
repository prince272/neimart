using System;
using System.Collections.Generic;
using System.Text;

namespace Neimart.Core.Services
{
    public static class EntityHelper
    {
        public static string GetCachePrefix<TEntity>()
            where TEntity : class, IEntity
        {
            var entityName = typeof(TEntity).Name;
            return $"Neimart.{entityName}";
        }
    }
}