using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;
using Neimart.Core.Entities;
using Neimart.Core.Filters;
using Neimart.Core.Utilities.Extensions;
using Neimart.Core.Utilities.Helpers;

namespace Neimart.Core.Services
{
    public class AddressService : EntityService<Address, AddressFilter>
    {
        public AddressService(IServiceProvider services) : base(services)
        {
        }

        public async Task ResolveAddressTypesAsync(Address currentAddress, IEnumerable<Address> otherAddresses)
        {
            otherAddresses = otherAddresses.Where(x => x.Id != currentAddress?.Id);
            var allAddressTypes = EnumHelper.GetEnumValues<AddressType>();
            var currentAddressTypes = allAddressTypes.Intersect(currentAddress?.AddressTypes ?? Enumerable.Empty<AddressType>()).ToList();

            if (currentAddress != null)
            {
                currentAddress.AddressTypes.Clear();
                currentAddress.AddressTypes.AddRange(currentAddressTypes);
                await UpdateAsync(currentAddress);
            }

            var remainingAddressTypes = allAddressTypes.Except(currentAddressTypes).ToList();

            foreach (var otherAddress in otherAddresses)
            {
                otherAddress.AddressTypes.Clear();
                await UpdateAsync(otherAddress);
            }
        }

        public override IQueryable<Address> GetQuery(AddressFilter filter = null)
        {
            var query = _unitOfWork.Query<Address>();

            query = query.OrderByDescending(x => x);

            if (filter != null)
            {
                if (filter.CustomerId != null)
                {
                    query = query.Where(x => x.CustomerId == filter.CustomerId);
                }

                if (filter.AddressId != null)
                {
                    query = query.Where(x => x.Id == filter.AddressId);
                }

                if (filter.AddressType != null || filter.AddressTypes != null)
                {
                    // TODO: Workaround to filter addresses by the address type 
                    // since ef core does not support storing primitive collection types yet.
                    var addressIdsWithTypes = query.Select(x => new { x.Id, x.AddressTypes }).ToList();
                    var addressIds = Enumerable.Empty<long>();

                    if (filter.AddressType != null)
                        addressIds = addressIdsWithTypes.Where(x => x.AddressTypes.Contains(filter.AddressType.Value)).Select(x => x.Id).ToArray();

                    else if (filter.AddressTypes != null)
                        addressIds = addressIdsWithTypes.Where(x => filter.AddressTypes.Any(type => x.AddressTypes.Contains(type))).Select(x => x.Id).ToArray();


                    query = query.Where(x => addressIds.Contains(x.Id));
                }
            }

            return query;
        }

        public async Task<CountEvaluation> EvaluateAsync(AddressFilter filter)
        {
            if (filter == null)
                throw new ArgumentNullException(nameof(filter));

            async Task<CountEvaluation> acquire()
            {
                var query = GetQuery(filter);

                return InternalEvaluate(count: await query.CountAsync());
            }

            string key = await _cacheManager.ComposeKeyAsync($"{EntityHelper.GetCachePrefix<Address>()}.{nameof(EvaluateAsync)}", filter);
            var result = await _cacheManager.GetAsync(key, acquire);
            return result;
        }
    }
}
