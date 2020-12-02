using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Neimart.Core.Infrastructure.Database
{
    public interface IUnitOfWork : IDisposable, IAsyncDisposable
    {
        Task CreateAsync<TEntity>(TEntity entity) where TEntity : class, IEntity;

        Task UpdateAsync<TEntity>(TEntity entity) where TEntity : class, IEntity;

        Task UpdateAsync<TEntity>(IEnumerable<TEntity> entities) where TEntity : class, IEntity;

        Task DeleteAsync<TEntity>(TEntity entity) where TEntity : class, IEntity;

        IQueryable<TEntity> Query<TEntity>() where TEntity : class, IEntity;

        Task<IUnitOfWorkTransaction> BeginAsync();
    }
}
