using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Neimart.Core.Entities;
using Neimart.Data.Extensions;

namespace Neimart.Data.Configurations
{
    public class OrderItemConfiguration : IEntityTypeConfiguration<OrderItem>
    {
        public void Configure(EntityTypeBuilder<OrderItem> builder)
        {
            builder.ToTable(nameof(OrderItem));

            builder.Property(x => x.Document).HasJsonConversion();
            builder.Property(x => x.Image).HasJsonConversion();
        }
    }
}
