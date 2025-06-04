using Ecom.Users.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Ecom.Users.Infrastructure.Persistence.EfCore.Configurations;

public class PermissionConfiguration : IEntityTypeConfiguration<Permission>
{
    public void Configure(EntityTypeBuilder<Permission> builder)
    {
        // Primary Key
        builder.HasKey(p => p.Id);

        // Properties
        builder.Property(p => p.Module).IsRequired().HasMaxLength(100);
        builder.Property(p => p.Action).IsRequired().HasMaxLength(100);
        builder.Property(p => p.Resource).HasMaxLength(100);
        builder.Property(p => p.Description).HasMaxLength(512);

        // Composite unique index
        builder.HasIndex(p => new { p.Module, p.Action, p.Resource }).IsUnique();

        // Audit fields from BaseEntity
        builder
            .Property(p => p.CreatedAt)
            .HasColumnType("timestamp with time zone")
            .HasConversion(v => v.UtcDateTime, v => DateTime.SpecifyKind(v, DateTimeKind.Utc))
            .IsRequired();

        builder
            .Property(p => p.UpdatedAt)
            .HasColumnType("timestamp with time zone")
            .HasConversion(
                v => v.HasValue ? v.Value.UtcDateTime : (DateTime?)null,
                v => v.HasValue ? DateTime.SpecifyKind(v.Value, DateTimeKind.Utc) : (DateTimeOffset?)null
            );

        builder
            .Property(p => p.DeletedAt)
            .HasColumnType("timestamp with time zone")
            .HasConversion(
                v => v.HasValue ? v.Value.UtcDateTime : (DateTime?)null,
                v => v.HasValue ? DateTime.SpecifyKind(v.Value, DateTimeKind.Utc) : (DateTimeOffset?)null
            );

        builder.Property(p => p.IsDeleted).HasDefaultValue(false);
        builder.Property(p => p.CreatedBy).IsRequired();
        builder.Property(p => p.UpdatedBy);
        builder.Property(p => p.DeletedBy);

        // Indexes
        builder.HasIndex(p => p.IsDeleted);
        builder.HasIndex(p => p.CreatedAt);
        builder.HasIndex(p => p.UpdatedAt);
        builder.HasIndex(p => p.Module);
        builder.HasIndex(p => p.Action);

        // Navigation properties
        builder
            .HasMany(p => p.RolePermissions)
            .WithOne(rp => rp.Permission)
            .HasForeignKey(rp => rp.PermissionId);

        // Global Query Filter for Soft Delete
        builder.HasQueryFilter(p => !p.IsDeleted);
    }
}