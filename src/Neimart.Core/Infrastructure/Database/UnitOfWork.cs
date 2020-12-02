using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Neimart.Core;

namespace Neimart.Core.Infrastructure.Database
{
    public class UnitOfWork<TContext> : IUnitOfWork
        where TContext : DbContext
    {
        private readonly TContext _dbContext;

        public UnitOfWork(TContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task CreateAsync<TEntity>(TEntity entity)
            where TEntity : class, IEntity
        {
            _dbContext.Add(entity);
            await _dbContext.SaveChangesAsync();

            await DetachAllEntitiesAsync();
        }

        public async Task UpdateAsync<TEntity>(TEntity entity)
            where TEntity : class, IEntity
        {
            // TODO: Consider checking whether the entity exists before we update.
            // dbContext.<Entity>.Update() is creating new records when key not set #10194
            // source: https://github.com/dotnet/efcore/issues/10194

            EnsureEntityKeyIsSet(entity);

            _dbContext.Update(entity);
            await _dbContext.SaveChangesAsync();

            await DetachAllEntitiesAsync();

        }

        public async Task UpdateAsync<TEntity>(IEnumerable<TEntity> entities) where TEntity : class, IEntity
        {
            foreach (var entity in entities)
                await UpdateAsync(entity);
        }

        public async Task DeleteAsync<TEntity>(TEntity entity)
            where TEntity : class, IEntity
        {
            EnsureEntityKeyIsSet(entity);

            _dbContext.Remove(entity);
            await _dbContext.SaveChangesAsync();

            await DetachAllEntitiesAsync();
        }

        public IQueryable<TEntity> Query<TEntity>()
            where TEntity : class, IEntity
        {
            return _dbContext.Set<TEntity>();
        }

        public async Task<IUnitOfWorkTransaction> BeginAsync()
        {
            var transaction = await _dbContext.Database.BeginTransactionAsync();
            return new UnitOfWorkTransaction(transaction);
        }

        private Task DetachAllEntitiesAsync()
        {
            var changedEntriesCopy = _dbContext.ChangeTracker.Entries().ToList();
            foreach (var entry in changedEntriesCopy)
                entry.State = EntityState.Detached;

            return Task.CompletedTask;
        }

        public void EnsureEntityKeyIsSet<TEntity>(TEntity entity)
            where TEntity : class, IEntity
        {
            if (entity.Id == 0)
            {
                throw new InvalidOperationException($"The entity value with the key {entity.Id} and of type {entity.GetType().Name} cannot be updated because it does not exist in the database.");
            }
        }

        #region IAsyncDisposable Support
        public virtual ValueTask DisposeAsync()
        {
            Dispose();
            return default;
        }
        #endregion

        #region IDisposable Support
        private bool disposed = false;

        protected virtual void Dispose(bool disposing)
        {
            if (disposed)
            {
                return;
            }

            if (disposing)
            {
            }

            disposed = true;
        }

        ~UnitOfWork()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        #endregion
    }
}