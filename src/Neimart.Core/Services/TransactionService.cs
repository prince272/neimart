using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Neimart.Core.Entities;
using Neimart.Core.Filters;
using Neimart.Core.Infrastructure.Paying;

namespace Neimart.Core.Services
{
    public class TransactionService : EntityService<Transaction, TransactionFilter>
    {
        public TransactionService(IServiceProvider services)
            : base(services)
        {
        }

        public override Task CreateAsync(Transaction transaction)
        {
            if (transaction == null)
                throw new ArgumentNullException(nameof(transaction));

            transaction.CreatedOn = DateTimeOffset.UtcNow;

            return base.CreateAsync(transaction);
        }

        public override Task UpdateAsync(Transaction transaction)
        {
            transaction.Updated = true;
            transaction.UpdatedOn = DateTimeOffset.UtcNow;

            return base.UpdateAsync(transaction);
        }

        public override IQueryable<Transaction> GetQuery(TransactionFilter filter = null)
        {
            var query = _unitOfWork.Query<Transaction>();

            query = query.OrderByDescending(x => !x.Updated ? x.CreatedOn : x.UpdatedOn);

            if (filter != null)
            {
                if (filter.TransactionCode != null)
                {
                    query = query.Where(x => !string.IsNullOrWhiteSpace(x.TransactionCode) && x.TransactionCode == filter.TransactionCode);
                }

                if (filter.TransactionId != null)
                {
                    query = query.Where(x => x.Id == filter.TransactionId);
                }

                if (filter.MemberId != null)
                {
                    query = query.Where(x => x.MemberId == filter.MemberId);
                }

                if (filter.Status != null)
                {
                    query = query.Where(x => x.Status == filter.Status);
                }

                if (filter.Processor != null)
                {
                    query = query.Where(x => x.Processor == filter.Processor);
                }

                if (filter.Mode != null)
                {
                    query = query.Where(x => x.Mode == filter.Mode);
                }

                if (filter.Type != null)
                {
                    query = query.Where(x => x.Type == filter.Type);
                }


                if (!string.IsNullOrWhiteSpace(filter.Search))
                {
                    query = query.Where(x => EF.Functions.Like(x.Reference, $"%{filter.Search}%") ||
                                             EF.Functions.Like(x.TransactionCode, $"%{filter.Search}%") ||
                                             EF.Functions.Like(x.AccountEmail, $"%{filter.Search}%") ||
                                             EF.Functions.Like(x.AccountNumber, $"%{filter.Search}%") ||
                                             EF.Functions.Like(x.IpAddress, $"%{filter.Search}%"));
                }
            }

            return query;
        }
    }
}