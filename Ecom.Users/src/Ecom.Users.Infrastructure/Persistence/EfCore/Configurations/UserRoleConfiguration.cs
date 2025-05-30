using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Ecom.Users.Infrastructure.Persistence.EfCore.Configurations;

public class UserRoleConfiguration : IEntityTypeConfiguration<UserRole>
{
    public void Configure(EntityTypeBuilder<UserRole> builder)
    {
        builder.ToTable("UserRoles", "users");
        
        builder.HasKey(ur => new { ur.UserId, ur.RoleId });
        
        builder.Property(ur => ur.AssignedAt)
            .IsRequired();
            
        builder.Property(ur => ur.AssignedBy)
            .IsRequired();
            
        builder.Property(ur => ur.IsActive)
            .IsRequired()
            .HasDefaultValue(true);
            
        builder.Property(ur => ur.ExpiresAt)
            .IsRequired(false);
        
        // Relationships
        builder.HasOne(ur => ur.User)
            .WithMany(u => u.UserRoles)
            .HasForeignKey(ur => ur.UserId)
            .OnDelete(DeleteBehavior.Cascade);
            
        builder.HasOne(ur => ur.Role)
            .WithMany(r => r.UserRoles)
            .HasForeignKey(ur => ur.RoleId)
            .OnDelete(DeleteBehavior.Cascade);
        
        // Indexes
        builder.HasIndex(ur => ur.AssignedAt);
        builder.HasIndex(ur => ur.IsActive);
        builder.HasIndex(ur => ur.ExpiresAt);
        
        // Seed data - assign default User role to seeded users
        builder.HasData(SeedUserRoles());
    }
    
    private static UserRole[] SeedUserRoles()
    {
        return new[]
        {
            // Admin user gets Admin role
            new UserRole
            {
                UserId = 1,
                RoleId = 1, // Admin role
                AssignedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                AssignedBy = 1, // Self-assigned
                IsActive = true
            },
            // Employee user gets Employee role
            new UserRole
            {
                UserId = 2,
                RoleId = 2, // Employee role
                AssignedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                AssignedBy = 1, // Assigned by admin
                IsActive = true
            },
            // Regular user gets User role
            new UserRole
            {
                UserId = 3,
                RoleId = 3, // User role
                AssignedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                AssignedBy = 1, // Assigned by admin
                IsActive = true
            }
        };
    }
}
