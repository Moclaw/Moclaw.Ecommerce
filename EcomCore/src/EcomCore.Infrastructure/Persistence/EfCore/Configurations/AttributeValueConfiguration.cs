using EcomCore.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EcomCore.Infrastructure.Persistence.EfCore.Configurations
{
    public class AttributeValueConfiguration : IEntityTypeConfiguration<AttributeValue>
    {
        public void Configure(EntityTypeBuilder<AttributeValue> builder)
        {
            // Primary Key
            builder.HasKey(av => av.Id);

            // Properties
            builder.Property(av => av.Value).IsRequired();

            builder.Property(av => av.AttributeId).IsRequired();

            builder
                .Property(av => av.CreatedAt)
                .HasColumnType("timestamp with time zone")
                .HasConversion(v => v.UtcDateTime, v => DateTime.SpecifyKind(v, DateTimeKind.Utc))
                .IsRequired();

            builder
                .Property(av => av.UpdatedAt)
                .HasColumnType("timestamp with time zone")
                .HasConversion(v => v.UtcDateTime, v => DateTime.SpecifyKind(v, DateTimeKind.Utc))
                .IsRequired();

            builder
                .Property(av => av.DeletedAt)
                .HasColumnType("timestamp with time zone")
                .HasConversion(
                    v => v.HasValue ? v.Value.UtcDateTime : (DateTime?)null,
                    v =>
                        v.HasValue
                            ? DateTime.SpecifyKind(v.Value, DateTimeKind.Utc)
                            : (DateTimeOffset?)null
                );

            builder.Property(av => av.IsDeleted).HasDefaultValue(false);

            builder.Property(av => av.CreatedBy).IsRequired();

            builder.Property(av => av.UpdatedBy);

            builder.Property(av => av.DeletedBy);

            // Relationships
            builder.HasOne(av => av.Attribute).WithMany().HasForeignKey(av => av.AttributeId);

            // Global Query Filter for Soft Delete
            builder.HasQueryFilter(av => !av.IsDeleted);
        }
    }
}
