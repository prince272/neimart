using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Neimart.Core.Entities;
using Neimart.Data.Extensions;

namespace Neimart.Data.Configurations
{
    public class OrderConfiguration : IEntityTypeConfiguration<Order>
    {
        public void Configure(EntityTypeBuilder<Order> builder)
        {
            builder.ToTable(nameof(Order));

            builder.Property(x => x.BillingAddress).HasJsonConversion();
            builder.Property(x => x.DeliveryAddress).HasJsonConversion();

            builder.HasQueryFilter(x => !x.Deleted);
        }
    }
}
