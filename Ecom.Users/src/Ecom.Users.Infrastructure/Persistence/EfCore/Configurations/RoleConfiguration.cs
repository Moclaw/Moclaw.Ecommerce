using Ecom.Users.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Ecom.Users.Infrastructure.Persistence.EfCore.Configurations;

public class RoleConfiguration : IEntityTypeConfiguration<Role>
{
    public void Configure(EntityTypeBuilder<Role> builder)
    {
        // Primary Key
        builder.HasKey(r => r.Id);

        // Properties
        builder.Property(r => r.Name).IsRequired().HasMaxLength(256);
        builder.HasIndex(r => r.Name).IsUnique();

        builder.Property(r => r.NormalizedName).HasMaxLength(256);
        builder.HasIndex(r => r.NormalizedName).IsUnique();

        builder.Property(r => r.Description).HasMaxLength(512);
        builder.Property(r => r.ConcurrencyStamp).HasMaxLength(256);

        // Audit fields from BaseEntity
        builder
            .Property(r => r.CreatedAt)
            .HasColumnType("timestamp with time zone")
            .HasConversion(v => v.UtcDateTime, v => DateTime.SpecifyKind(v, DateTimeKind.Utc))
            .IsRequired();

        builder
            .Property(r => r.UpdatedAt)
            .HasColumnType("timestamp with time zone")
            .HasConversion(
                v => v.HasValue ? v.Value.UtcDateTime : (DateTime?)null,
                v => v.HasValue ? DateTime.SpecifyKind(v.Value, DateTimeKind.Utc) : (DateTimeOffset?)null
            );

        builder
            .Property(r => r.DeletedAt)
            .HasColumnType("timestamp with time zone")
            .HasConversion(
                v => v.HasValue ? v.Value.UtcDateTime : (DateTime?)null,
                v => v.HasValue ? DateTime.SpecifyKind(v.Value, DateTimeKind.Utc) : (DateTimeOffset?)null
            );

        builder.Property(r => r.IsDeleted).HasDefaultValue(false);
        builder.Property(r => r.CreatedBy).IsRequired();
        builder.Property(r => r.UpdatedBy);
        builder.Property(r => r.DeletedBy);

        // Indexes
        builder.HasIndex(r => r.IsDeleted);
        builder.HasIndex(r => r.CreatedAt);
        builder.HasIndex(r => r.UpdatedAt);

        // Navigation properties
        builder
            .HasMany(r => r.UserRoles)
            .WithOne(ur => ur.Role)
            .HasForeignKey(ur => ur.RoleId);

        builder
            .HasMany(r => r.RolePermissions)
            .WithOne(rp => rp.Role)
            .HasForeignKey(rp => rp.RoleId);

        // Global Query Filter for Soft Delete
        builder.HasQueryFilter(r => !r.IsDeleted);
    }
}