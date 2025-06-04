using Ecom.Users.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Ecom.Users.Infrastructure.Persistence.EfCore.Configurations;

public class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshToken>
{
    public void Configure(EntityTypeBuilder<RefreshToken> builder)
    {
        // Primary Key
        builder.HasKey(rt => rt.Id);

        // Properties
        builder.Property(rt => rt.UserId).IsRequired();
        builder.Property(rt => rt.Token).IsRequired().HasMaxLength(512);
        builder.HasIndex(rt => rt.Token).IsUnique();

        builder.Property(rt => rt.JwtId).HasMaxLength(256);
        builder.HasIndex(rt => rt.JwtId);

        builder
            .Property(rt => rt.ExpiryDate)
            .HasColumnType("timestamp with time zone")
            .HasConversion(v => v.UtcDateTime, v => DateTime.SpecifyKind(v, DateTimeKind.Utc))
            .IsRequired();

        builder
            .Property(rt => rt.RevokedAt)
            .HasColumnType("timestamp with time zone")
            .HasConversion(
                v => v.HasValue ? v.Value.UtcDateTime : (DateTime?)null,
                v => v.HasValue ? DateTime.SpecifyKind(v.Value, DateTimeKind.Utc) : (DateTimeOffset?)null
            );

        builder.Property(rt => rt.IsUsed).HasDefaultValue(false);
        builder.Property(rt => rt.IsRevoked).HasDefaultValue(false);

        // Audit fields from BaseEntity
        builder
            .Property(rt => rt.CreatedAt)
            .HasColumnType("timestamp with time zone")
            .HasConversion(v => v.UtcDateTime, v => DateTime.SpecifyKind(v, DateTimeKind.Utc))
            .IsRequired();

        builder
            .Property(rt => rt.UpdatedAt)
            .HasColumnType("timestamp with time zone")
            .HasConversion(
                v => v.HasValue ? v.Value.UtcDateTime : (DateTime?)null,
                v => v.HasValue ? DateTime.SpecifyKind(v.Value, DateTimeKind.Utc) : (DateTimeOffset?)null
            );

        builder
            .Property(rt => rt.DeletedAt)
            .HasColumnType("timestamp with time zone")
            .HasConversion(
                v => v.HasValue ? v.Value.UtcDateTime : (DateTime?)null,
                v => v.HasValue ? DateTime.SpecifyKind(v.Value, DateTimeKind.Utc) : (DateTimeOffset?)null
            );

        builder.Property(rt => rt.IsDeleted).HasDefaultValue(false);
        builder.Property(rt => rt.CreatedBy).IsRequired();
        builder.Property(rt => rt.UpdatedBy);
        builder.Property(rt => rt.DeletedBy);

        // Indexes
        builder.HasIndex(rt => rt.IsDeleted);
        builder.HasIndex(rt => rt.CreatedAt);
        builder.HasIndex(rt => rt.UpdatedAt);
        builder.HasIndex(rt => rt.UserId);
        builder.HasIndex(rt => rt.ExpiryDate);
        builder.HasIndex(rt => rt.IsUsed);
        builder.HasIndex(rt => rt.IsRevoked);

        // Relationships
        builder
            .HasOne(rt => rt.User)
            .WithMany(u => u.RefreshTokens)
            .HasForeignKey(rt => rt.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        // Global Query Filter for Soft Delete
        builder.HasQueryFilter(rt => !rt.IsDeleted);
    }
}