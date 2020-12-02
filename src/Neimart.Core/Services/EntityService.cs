using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Neimart.Core.Entities;
using Neimart.Core.Events;
using Neimart.Core.Filters;
using Neimart.Core.Infrastructure.Caching;
using Neimart.Core.Infrastructure.Database;
using Neimart.Core.Settings;
using Neimart.Core.Utilities;
using Neimart.Core.Utilities.Helpers;

namespace Neimart.Core.Services
{
    public abstract class EntityService<TEntity, TEntityFilter>
        where TEntity : class, IEntity
        where TEntityFilter : class
    {
        protected readonly IUnitOfWork _unitOfWork;
        protected readonly IMediator _mediator;
        protected readonly ICacheManager _cacheManager;
        protected readonly AppSettings _appSettings;

        public EntityService(IServiceProvider services)
        {
            _unitOfWork = services.GetRequiredService<IUnitOfWork>();
            _mediator = services.GetRequiredService<IMediator>();
            _cacheManager = services.GetRequiredService<ICacheManager>();
            _appSettings = services.GetRequiredService<IOptions<AppSettings>>().Value;
        }

        public virtual async Task CreateAsync(TEntity entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            await PrepareAsync(entity);
            await _unitOfWork.CreateAsync(entity);
            await ClearCacheAsync();
        }

        public virtual async Task UpdateAsync(TEntity entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            await PrepareAsync(entity);
            await _unitOfWork.UpdateAsync(entity);
            await ClearCacheAsync();
        }

        public virtual async Task DeleteAsync(TEntity entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            await _unitOfWork.DeleteAsync(entity);
            await ClearCacheAsync();
        }

        public virtual Task<bool> ExistsAsync(TEntity entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            var query = _unitOfWork.Query<TEntity>();

            return query.AnyAsync(x => x.Id == entity.Id);
        }

        protected virtual Task PrepareAsync(TEntity entity)
        {
            return Task.CompletedTask;
        }

        public virtual async Task ClearCacheAsync()
        {
            await _cacheManager.RemoveByPrefixAsync(EntityHelper.GetCachePrefix<TEntity>());
        }

        public virtual async Task<TEntity> GetAsync(TEntityFilter filter)
        {
            return (await ListAsync(filter)).FirstOrDefault();
        }

        public virtual async Task<IEnumerable<TEntity>> ListAsync(TEntityFilter filter)
        {
            var query = GetQuery(filter);
            var result = await query.ToListAsync();
            return result;
        }

        public abstract IQueryable<TEntity> GetQuery(TEntityFilter filter);

        protected AmountEvaluation InternalEvaluate(decimal amount = 0, int quantity = 1, int count = 1)
        {
            return new AmountEvaluation
            {
                TotalAmount = amount,
                TotalCount = count,
                TotalQuantity = quantity,
            };
        }
    }

    public class AmountEvaluation : CountEvaluation
    {
        public decimal TotalAmount { get; set; }

        public int TotalQuantity { get; set; }
    }

    public class CountEvaluation
    {
        public int TotalCount { get; set; }
    }
}