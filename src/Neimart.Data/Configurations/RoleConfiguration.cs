using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Neimart.Core.Entities;
using Neimart.Data.Extensions;

namespace Neimart.Data.Configurations
{
    public class RoleConfiguration : IEntityTypeConfiguration<Role>
    {
        public void Configure(EntityTypeBuilder<Role> builder)
        {
            builder.ToTable(nameof(Role));

            builder.HasMany(x => x.UserRoles)
                   .WithOne(x => x.Role)
                   .HasForeignKey(x => x.RoleId)
                   .IsRequired();

            #region Role
            builder.HasData(() => new Role
            {
                Id = 1,
                Name = "Admin",
                NormalizedName = "ADMIN",
                ConcurrencyStamp = "75c2bd04-8d93-40ec-9a8d-63a72ff765dd",
            });
            builder.HasData(() => new Role
            {
                Id = 2,
                Name = "Seller",
                NormalizedName = "SELLER",
                ConcurrencyStamp = "6bfe8274-a6af-4328-b192-a39c90d2c796",
            });
            builder.HasData(() => new Role
            {
                Id = 3,
                Name = "Customer",
                NormalizedName = "CUSTOMER",
                ConcurrencyStamp = "f40c6c2d-7251-4fd0-8667-e8f4828bb440",
            });
            builder.HasData(() => new Role
            {
                Id = 4,
                Name = "Guest",
                NormalizedName = "GUEST",
                ConcurrencyStamp = "0ba2381a-0821-4385-9099-9ddca7a444d8",
            });
            #endregion
        }
    }
}
