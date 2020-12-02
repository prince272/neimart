using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Neimart.Core.Entities;
using Neimart.Core.Filters;
using Neimart.Core.Infrastructure.Database;
using Neimart.Core.Settings;
using Neimart.Core.Utilities;
using Neimart.Core.Utilities.Helpers;

namespace Neimart.Core.Services
{
    public class UserService : UserManager<User>
    {
        private readonly IServiceProvider _services;
        private readonly IUnitOfWork _unitOfWork;
        private readonly AppSettings _appSettings;

        public UserService(IUserStore<User> store,
                           IOptions<IdentityOptions> optionsAccessor,
                           IPasswordHasher<User> passwordHasher,
                           IEnumerable<IUserValidator<User>> userValidators,
                           IEnumerable<IPasswordValidator<User>> passwordValidators,
                           ILookupNormalizer keyNormalizer,
                           IdentityErrorDescriber errors,
                           IServiceProvider services,
                           ILogger<UserManager<User>> logger) : base(store, optionsAccessor, passwordHasher, userValidators, passwordValidators, keyNormalizer, errors, services, logger)
        {
            _services = services;
            _unitOfWork = services.GetRequiredService<IUnitOfWork>();
            _appSettings = services.GetRequiredService<IOptions<AppSettings>>().Value;
        }


        public override async Task<IdentityResult> CreateAsync(User user)
        {
            if (user == null)
                throw new ArgumentNullException(nameof(user));

            user.CreatedOn = DateTimeOffset.UtcNow;

            return await base.CreateAsync(user);
        }

        public override async Task<IdentityResult> CreateAsync(User user, string password)
        {
            if (user == null)
                throw new ArgumentNullException(nameof(user));

            user.CreatedOn = DateTimeOffset.UtcNow;

            return await base.CreateAsync(user, password);
        }

        public override async Task<IdentityResult> UpdateAsync(User user)
        {
            user.UpdatedOn = DateTimeOffset.UtcNow;
            user.Updated = true;

            return await base.UpdateAsync(user);
        }

        public async Task<IdentityResult> CheckStoreSlugAsync(User user)
        {
            if (await Users.AnyAsync(x => x.Id != user.Id && x.StoreSlug == user.StoreSlug))
            {
                return IdentityResult.Failed(new IdentityError { Code = "DuplicateStoreSlug", Description = "The store slug is already taken." });
            }

            return IdentityResult.Success;
        }

        public Task<User> FindByIdAsync(long id)
        {
            return Users.FirstOrDefaultAsync(x => x.Id == id);
        }

        public Task<User> FindByCodeAsync(string userCode)
        {
            return Users.FirstOrDefaultAsync(x => x.UserCode == userCode);
        }

        public async Task<User> FindByStoreSlugAsync(string storeSlug)
        {
            return await Users.FirstOrDefaultAsync(x => x.StoreSlug == storeSlug);
        }

        public Task GenerateStoreSlugAsync(User user)
        {
            user.StoreSlug = SanitizerHelper.GenerateSlug($"{user.StoreName}",
                             slug => Users.Any(x => x.Id != user.Id && x.StoreSlug == slug));

            return Task.CompletedTask;
        }

        public Task GenerateUserNameAsync(User user)
        {
            user.UserName = SanitizerHelper.GenerateSlug($"{user.FirstName} {user.LastName}",
                             userName => Users.Any(x => x.Id != user.Id && x.UserName == userName));
            return Task.CompletedTask;
        }

        public Task GenerateUserCodeAsync(User user)
        {
            user.UserCode = _appSettings.GenerateCode("User", user.FirstName);
            return Task.CompletedTask;
        }

        public IQueryable<User> GetQuery(UserFilter filter)
        {
            var query = Users.AsQueryable();

            query = query.OrderByDescending(x => ((int)x.StorePlanType))
                         .OrderByDescending(x => ((int)x.StorePlanPeriod));

            query = query.OrderByDescending(x => !x.Updated ? x.CreatedOn : x.UpdatedOn);

            if (filter != null)
            {
                if (filter.StoreAccess != null)
                {
                    query = query.Where(x => x.StoreAccess == filter.StoreAccess);
                }

                if (filter.StoreStatus != null)
                {
                    query = query.Where(x => x.StoreStatus == filter.StoreStatus);
                }

                if (filter.StoreCategory != null)
                {
                    query = query.Where(x => x.StoreCategory.HasFlag(filter.StoreCategory.Value));
                }

                if (filter.StoreSetup != null)
                {
                    query = query.Where(x => x.StoreSetup == filter.StoreSetup);
                }

                if (filter.StoreRegion != null)
                {
                    query = query.Where(x => x.StoreRegion == filter.StoreRegion);
                }

                if (filter.StorePlace != null)
                {
                    query = query.Where(x => x.StorePlace == filter.StorePlace);
                }

                if (filter.StorePlanActive != null)
                {
                    query = query.Where(x => (DateTimeOffset.UtcNow < x.StorePlanEndedOn) == filter.StorePlanActive);
                }

                if (!string.IsNullOrWhiteSpace(filter.Search))
                {
                    query = query.Where(x =>
                    EF.Functions.Like(x.StoreName, $"%{filter.Search}%") ||
                    x.Products.Any(p => EF.Functions.Like(p.Name, $"%{filter.Search}%")));
                }
            }

            return query;
        }

        public override IQueryable<User> Users => base.Users.Include(x => x.StoreDocument).Include(x => x.StoreLogo).Include(x => x.UserImage);

        public Result CanDeposit(User member, decimal amount)
        {
            if (member == null)
                throw new ArgumentNullException(nameof(member));

            if (amount <= 0)
                return Result.Fail("Amount must be greater then zero.");

            if (amount > _appSettings.CurrencyMaxValue)
                return Result.Fail($"Amount must be less then {_appSettings.CurrencyMaxValue}.");

            return Result.Ok();
        }

        public async Task<Result> DepositAsync(User member, decimal amount)
        {
            var result = CanDeposit(member, amount);
            if (!result.Success) return result;

            member.Balance += amount;
            await UpdateAsync(member);

            return Result.Ok();
        }

        public Result CanWithdraw(User member, decimal amount)
        {
            if (member == null)
                throw new ArgumentNullException(nameof(member));

            if (amount <= 0)
                return Result.Fail("Amount must be greater then zero.");

            if (amount > _appSettings.CurrencyMaxValue)
                return Result.Fail($"Amount must be less then {_appSettings.CurrencyMaxValue}.");

            if (member.Balance < amount)
                return Result.Fail($"Balance is insufficient.");

            return Result.Ok();
        }

        public async Task<Result> WithdrawAsync(User member, decimal amount)
        {
            var result = CanWithdraw(member, amount);
            if (!result.Success) return result;

            member.Balance -= amount;
            await UpdateAsync(member);

            return Result.Ok();
        }

        public async Task<Result> TransferAsync(User sender, User receiver, decimal amount)
        {
            var result = await WithdrawAsync(sender, amount);

            if (result.Success)
            {
                result = await DepositAsync(receiver, amount);
            }

            return result;
        }

        public Task<Delivery> GetDeliveryAsync(User user, Address address)
        {
            if (user == null)
                throw new ArgumentNullException(nameof(user));

            if (address == null)
                throw new ArgumentNullException(nameof(address));

            var delivery = user.StoreDeliveries
                .FirstOrDefault(x => (x.Region == null || x.Region == address.Region) &&
                                     (x.Place == null || x.Place == address.Place));
            return Task.FromResult(delivery);
        }
    }
}