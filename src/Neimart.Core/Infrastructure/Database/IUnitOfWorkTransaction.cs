using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Neimart.Core.Infrastructure.Database
{
    public interface IUnitOfWorkTransaction : IDisposable, IAsyncDisposable
    {
        Guid Id { get; }

        Task CommitAsync();

        Task RollbackAsync();
    }
}
