using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Neimart.Core.Entities;
using Neimart.Data.Extensions;

namespace Neimart.Data.Configurations
{
    public class UserConfiguration : IEntityTypeConfiguration<User>
    {
        public void Configure(EntityTypeBuilder<User> builder)
        {
            builder.ToTable(nameof(User));

            builder.HasMany(x => x.UserRoles)
                   .WithOne(x => x.User)
                   .HasForeignKey(x => x.UserId)
                   .IsRequired();

            builder.Property(x => x.StoreDeliveries).HasJsonConversion();

            builder.HasQueryFilter(x => !x.Deleted);
        }
    }
}
