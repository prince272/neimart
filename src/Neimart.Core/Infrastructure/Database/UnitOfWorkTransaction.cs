using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Storage;
using Neimart.Core;

namespace Neimart.Core.Infrastructure.Database
{
    public class UnitOfWorkTransaction : IUnitOfWorkTransaction
    {

        private readonly IDbContextTransaction _innerTransaction;

        public UnitOfWorkTransaction(IDbContextTransaction transaction)
        {
            _innerTransaction = transaction;
        }

        public Guid Id => _innerTransaction.TransactionId;

        public void Dispose() => _innerTransaction.Dispose();

        public ValueTask DisposeAsync() => _innerTransaction.DisposeAsync();

        public Task CommitAsync() => _innerTransaction.CommitAsync();

        public Task RollbackAsync() => _innerTransaction.RollbackAsync();
    }
}
