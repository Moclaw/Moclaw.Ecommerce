using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using EcomCore.Domain.Entities;

namespace EcomCore.Infrastructure.Persistence.EfCore.Configurations
{
    public class AttributeConfiguration
        : IEntityTypeConfiguration<EcomCore.Domain.Entities.Attribute>
    {
        public void Configure(EntityTypeBuilder<EcomCore.Domain.Entities.Attribute> builder)
        {
            // Primary Key
            builder.HasKey(a => a.Id);

            // Properties
            builder.Property(a => a.Name).IsRequired();

            builder
                .Property(a => a.CreatedAt)
                .HasColumnType("timestamp with time zone")
                .HasConversion(v => v.UtcDateTime, v => DateTime.SpecifyKind(v, DateTimeKind.Utc))
                .IsRequired();

            builder
                .Property(a => a.UpdatedAt)
                .HasColumnType("timestamp with time zone")
                .HasConversion(v => v.UtcDateTime, v => DateTime.SpecifyKind(v, DateTimeKind.Utc))
                .IsRequired();

            builder
                .Property(a => a.DeletedAt)
                .HasColumnType("timestamp with time zone")
                .HasConversion(
                    v => v.HasValue ? v.Value.UtcDateTime : (DateTime?)null,
                    v =>
                        v.HasValue
                            ? DateTime.SpecifyKind(v.Value, DateTimeKind.Utc)
                            : (DateTimeOffset?)null
                );

            builder.Property(a => a.IsDeleted).HasDefaultValue(false);

            builder.Property(a => a.CreatedBy).IsRequired();

            builder.Property(a => a.UpdatedBy);

            builder.Property(a => a.DeletedBy);

            // Global Query Filter for Soft Delete
            builder.HasQueryFilter(a => !a.IsDeleted);
        }
    }
}
