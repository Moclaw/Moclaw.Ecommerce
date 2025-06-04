using Ecom.Users.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Ecom.Users.Infrastructure.Persistence.EfCore.Configurations;

public class RolePermissionConfiguration : IEntityTypeConfiguration<RolePermission>
{
    public void Configure(EntityTypeBuilder<RolePermission> builder)
    {
        // Primary Key
        builder.HasKey(rp => rp.Id);

        // Properties
        builder.Property(rp => rp.RoleId).IsRequired();
        builder.Property(rp => rp.PermissionId).IsRequired();

        // Composite unique index to prevent duplicate role-permission assignments
        builder.HasIndex(rp => new { rp.RoleId, rp.PermissionId }).IsUnique();

        // Audit fields from BaseEntity
        builder
            .Property(rp => rp.CreatedAt)
            .HasColumnType("timestamp with time zone")
            .HasConversion(v => v.UtcDateTime, v => DateTime.SpecifyKind(v, DateTimeKind.Utc))
            .IsRequired();

        builder
            .Property(rp => rp.UpdatedAt)
            .HasColumnType("timestamp with time zone")
            .HasConversion(
                v => v.HasValue ? v.Value.UtcDateTime : (DateTime?)null,
                v => v.HasValue ? DateTime.SpecifyKind(v.Value, DateTimeKind.Utc) : (DateTimeOffset?)null
            );

        builder
            .Property(rp => rp.DeletedAt)
            .HasColumnType("timestamp with time zone")
            .HasConversion(
                v => v.HasValue ? v.Value.UtcDateTime : (DateTime?)null,
                v => v.HasValue ? DateTime.SpecifyKind(v.Value, DateTimeKind.Utc) : (DateTimeOffset?)null
            );

        builder.Property(rp => rp.IsDeleted).HasDefaultValue(false);
        builder.Property(rp => rp.CreatedBy).IsRequired();
        builder.Property(rp => rp.UpdatedBy);
        builder.Property(rp => rp.DeletedBy);

        // Indexes
        builder.HasIndex(rp => rp.IsDeleted);
        builder.HasIndex(rp => rp.CreatedAt);
        builder.HasIndex(rp => rp.UpdatedAt);
        builder.HasIndex(rp => rp.RoleId);
        builder.HasIndex(rp => rp.PermissionId);

        // Relationships
        builder
            .HasOne(rp => rp.Role)
            .WithMany(r => r.RolePermissions)
            .HasForeignKey(rp => rp.RoleId)
            .OnDelete(DeleteBehavior.Cascade);

        builder
            .HasOne(rp => rp.Permission)
            .WithMany(p => p.RolePermissions)
            .HasForeignKey(rp => rp.PermissionId)
            .OnDelete(DeleteBehavior.Cascade);

        // Global Query Filter for Soft Delete
        builder.HasQueryFilter(rp => !rp.IsDeleted);
    }
}