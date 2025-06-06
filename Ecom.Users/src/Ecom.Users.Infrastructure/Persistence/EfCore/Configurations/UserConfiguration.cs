using Ecom.Users.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Ecom.Users.Infrastructure.Persistence.EfCore.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        // Primary Key
        builder.HasKey(u => u.Id);

        // Properties
        builder.Property(u => u.Email).IsRequired().HasMaxLength(256);
        builder.HasIndex(u => u.Email).IsUnique();

        builder.Property(u => u.UserName).HasMaxLength(256);
        builder.HasIndex(u => u.UserName).IsUnique();

        builder.Property(u => u.FirstName).HasMaxLength(100);
        builder.Property(u => u.LastName).HasMaxLength(100);
        builder.Property(u => u.PhoneNumber).HasMaxLength(20);
        builder.Property(u => u.PasswordHash).IsRequired().HasMaxLength(512);
        builder.Property(u => u.SecurityStamp).HasMaxLength(256);
        builder.Property(u => u.ConcurrencyStamp).HasMaxLength(256);
        builder.Property(u => u.Provider).HasMaxLength(50);
        builder.Property(u => u.ProviderId).HasMaxLength(256);

        // Default values
        builder.Property(u => u.EmailConfirmed).HasDefaultValue(false);
        builder.Property(u => u.PhoneNumberConfirmed).HasDefaultValue(false);
        builder.Property(u => u.TwoFactorEnabled).HasDefaultValue(false);
        builder.Property(u => u.LockoutEnabled).HasDefaultValue(true);
        builder.Property(u => u.AccessFailedCount).HasDefaultValue(0);

        // Audit fields from BaseEntity
        builder
            .Property(u => u.CreatedAt)
            .HasColumnType("timestamp with time zone")
            .HasConversion(v => v.UtcDateTime, v => DateTime.SpecifyKind(v, DateTimeKind.Utc))
            .IsRequired();

        builder
            .Property(u => u.UpdatedAt)
            .HasColumnType("timestamp with time zone")
            .HasConversion(
                v => v.HasValue ? v.Value.UtcDateTime : (DateTime?)null,
                v => v.HasValue ? DateTime.SpecifyKind(v.Value, DateTimeKind.Utc) : (DateTimeOffset?)null
            );

        builder
            .Property(u => u.DeletedAt)
            .HasColumnType("timestamp with time zone")
            .HasConversion(
                v => v.HasValue ? v.Value.UtcDateTime : (DateTime?)null,
                v => v.HasValue ? DateTime.SpecifyKind(v.Value, DateTimeKind.Utc) : (DateTimeOffset?)null
            );

        builder
            .Property(u => u.LockoutEnd)
            .HasColumnType("timestamp with time zone")
            .HasConversion(
                v => v.HasValue ? v.Value.UtcDateTime : (DateTime?)null,
                v => v.HasValue ? DateTime.SpecifyKind(v.Value, DateTimeKind.Utc) : (DateTimeOffset?)null
            );

        builder.Property(u => u.IsDeleted).HasDefaultValue(false);
        builder.Property(u => u.CreatedBy).IsRequired();
        builder.Property(u => u.UpdatedBy);
        builder.Property(u => u.DeletedBy);

        // Indexes
        builder.HasIndex(u => u.IsDeleted);
        builder.HasIndex(u => u.CreatedAt);
        builder.HasIndex(u => u.UpdatedAt);
        builder.HasIndex(u => u.Provider);
        builder.HasIndex(u => u.ProviderId);

        // Navigation properties
        builder
            .HasMany(u => u.UserRoles)
            .WithOne(ur => ur.User)
            .HasForeignKey(ur => ur.UserId);

        builder
            .HasMany(u => u.RefreshTokens)
            .WithOne(rt => rt.User)
            .HasForeignKey(rt => rt.UserId);

        // Global Query Filter for Soft Delete
        builder.HasQueryFilter(u => !u.IsDeleted);
    }
}