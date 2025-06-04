using Ecom.Users.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Ecom.Users.Infrastructure.Persistence.EfCore.Configurations;

public class UserRoleConfiguration : IEntityTypeConfiguration<UserRole>
{
    public void Configure(EntityTypeBuilder<UserRole> builder)
    {
        // Primary Key
        builder.HasKey(ur => ur.Id);

        // Properties
        builder.Property(ur => ur.UserId).IsRequired();
        builder.Property(ur => ur.RoleId).IsRequired();

        // Composite unique index to prevent duplicate user-role assignments
        builder.HasIndex(ur => new { ur.UserId, ur.RoleId }).IsUnique();

        // Audit fields from BaseEntity
        builder
            .Property(ur => ur.CreatedAt)
            .HasColumnType("timestamp with time zone")
            .HasConversion(v => v.UtcDateTime, v => DateTime.SpecifyKind(v, DateTimeKind.Utc))
            .IsRequired();

        builder
            .Property(ur => ur.UpdatedAt)
            .HasColumnType("timestamp with time zone")
            .HasConversion(
                v => v.HasValue ? v.Value.UtcDateTime : (DateTime?)null,
                v => v.HasValue ? DateTime.SpecifyKind(v.Value, DateTimeKind.Utc) : (DateTimeOffset?)null
            );

        builder
            .Property(ur => ur.DeletedAt)
            .HasColumnType("timestamp with time zone")
            .HasConversion(
                v => v.HasValue ? v.Value.UtcDateTime : (DateTime?)null,
                v => v.HasValue ? DateTime.SpecifyKind(v.Value, DateTimeKind.Utc) : (DateTimeOffset?)null
            );

        builder.Property(ur => ur.IsDeleted).HasDefaultValue(false);
        builder.Property(ur => ur.CreatedBy).IsRequired();
        builder.Property(ur => ur.UpdatedBy);
        builder.Property(ur => ur.DeletedBy);

        // Indexes
        builder.HasIndex(ur => ur.IsDeleted);
        builder.HasIndex(ur => ur.CreatedAt);
        builder.HasIndex(ur => ur.UpdatedAt);
        builder.HasIndex(ur => ur.UserId);
        builder.HasIndex(ur => ur.RoleId);

        // Relationships
        builder
            .HasOne(ur => ur.User)
            .WithMany(u => u.UserRoles)
            .HasForeignKey(ur => ur.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder
            .HasOne(ur => ur.Role)
            .WithMany(r => r.UserRoles)
            .HasForeignKey(ur => ur.RoleId)
            .OnDelete(DeleteBehavior.Cascade);

        // Global Query Filter for Soft Delete
        builder.HasQueryFilter(ur => !ur.IsDeleted);
    }
}