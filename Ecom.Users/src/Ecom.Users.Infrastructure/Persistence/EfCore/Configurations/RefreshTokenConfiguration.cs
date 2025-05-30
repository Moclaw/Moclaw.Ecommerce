using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Ecom.Users.Infrastructure.Persistence.EfCore.Configurations;

public class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshToken>
{
    public void Configure(EntityTypeBuilder<RefreshToken> builder)
    {
        builder.ToTable("RefreshTokens", "users");
        
        builder.HasKey(rt => rt.Id);
        
        builder.Property(rt => rt.Id)
            .ValueGeneratedOnAdd();
            
        builder.Property(rt => rt.Token)
            .IsRequired()
            .HasMaxLength(500);
            
        builder.Property(rt => rt.JwtId)
            .IsRequired()
            .HasMaxLength(36);
            
        builder.Property(rt => rt.CreatedAt)
            .IsRequired();
            
        builder.Property(rt => rt.ExpiresAt)
            .IsRequired();
            
        builder.Property(rt => rt.Used)
            .IsRequired()
            .HasDefaultValue(false);
            
        builder.Property(rt => rt.Invalidated)
            .IsRequired()
            .HasDefaultValue(false);
            
        builder.Property(rt => rt.UsedAt)
            .IsRequired(false);
            
        builder.Property(rt => rt.InvalidatedAt)
            .IsRequired(false);
            
        builder.Property(rt => rt.DeviceInfo)
            .HasMaxLength(255);
            
        builder.Property(rt => rt.IpAddress)
            .HasMaxLength(45); // IPv6 max length
        
        // Relationships
        builder.HasOne(rt => rt.User)
            .WithMany(u => u.RefreshTokens)
            .HasForeignKey(rt => rt.UserId)
            .OnDelete(DeleteBehavior.Cascade);
        
        // Indexes
        builder.HasIndex(rt => rt.Token)
            .IsUnique();
            
        builder.HasIndex(rt => rt.JwtId)
            .IsUnique();
            
        builder.HasIndex(rt => rt.UserId);
        builder.HasIndex(rt => rt.ExpiresAt);
        builder.HasIndex(rt => rt.Used);
        builder.HasIndex(rt => rt.Invalidated);
        builder.HasIndex(rt => new { rt.UserId, rt.Used, rt.Invalidated });
    }
}
