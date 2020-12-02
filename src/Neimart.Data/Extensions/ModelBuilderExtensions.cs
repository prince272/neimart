using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Neimart.Core;
using Neimart.Core.Entities;

namespace Neimart.Data.Extensions
{
    public static class ModelBuilderExtensions
    {
        // Disable cascade delete on EF Core 2 globally
        // source: https://stackoverflow.com/questions/46526230/disable-cascade-delete-on-ef-core-2-globally
        public static void EnableCascadeDelete(this ModelBuilder modelBuilder, bool cascadeDeleteEnabled = true)
        {
            if (cascadeDeleteEnabled)
            {
                var restrictFKs = modelBuilder.Model.GetEntityTypes()
                    .SelectMany(t => t.GetForeignKeys())
                    .Where(fk => !fk.IsOwnership && fk.DeleteBehavior == DeleteBehavior.Restrict);

                foreach (var fk in restrictFKs)
                    fk.DeleteBehavior = DeleteBehavior.Cascade;
            }
            else
            {
                var cascadeFKs = modelBuilder.Model.GetEntityTypes()
                    .SelectMany(t => t.GetForeignKeys())
                    .Where(fk => !fk.IsOwnership && fk.DeleteBehavior == DeleteBehavior.Cascade);

                foreach (var fk in cascadeFKs)
                    fk.DeleteBehavior = DeleteBehavior.Restrict;
            }
        }

        // Entity Framework Core - setting the decimal precision and scale to all decimal properties [duplicate]
        // source: https://stackoverflow.com/questions/43277154/entity-framework-core-setting-the-decimal-precision-and-scale-to-all-decimal-p
        public static void SetColumnTypeForAllProperties<T>(this ModelBuilder modelBuilder, string value)
        {
            foreach (var property in modelBuilder.Model.GetEntityTypes()
                .SelectMany(t => t.GetProperties())
                .Where(p => p.ClrType == typeof(T) || p.ClrType == Nullable.GetUnderlyingType(typeof(T))))
            {
                property.SetColumnType(value);
            }
        }

        // Use this method extensions to instead of the default 'HasData' method to prevent stack overflow exception.
        // source: https://github.com/aspnet/EntityFrameworkCore/issues/14702
        public static DataBuilder<TEntity> HasData<TEntity>(this EntityTypeBuilder<TEntity> source, params Func<object>[] dataFuncs)
            where TEntity : class, IEntity
        {
            return source.HasData(dataFuncs.Select(func => func()));
        }
    }
}
