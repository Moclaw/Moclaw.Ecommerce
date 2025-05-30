using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Ecom.Users.Infrastructure.Persistence.EfCore.Configurations;

public class RolePermissionConfiguration : IEntityTypeConfiguration<RolePermission>
{
    public void Configure(EntityTypeBuilder<RolePermission> builder)
    {
        builder.ToTable("RolePermissions", "users");
        
        builder.HasKey(rp => new { rp.RoleId, rp.PermissionId });
        
        builder.Property(rp => rp.GrantedAt)
            .IsRequired();
            
        builder.Property(rp => rp.GrantedBy)
            .IsRequired();
            
        builder.Property(rp => rp.IsActive)
            .IsRequired()
            .HasDefaultValue(true);
        
        // Relationships
        builder.HasOne(rp => rp.Role)
            .WithMany(r => r.RolePermissions)
            .HasForeignKey(rp => rp.RoleId)
            .OnDelete(DeleteBehavior.Cascade);
            
        builder.HasOne(rp => rp.Permission)
            .WithMany(p => p.RolePermissions)
            .HasForeignKey(rp => rp.PermissionId)
            .OnDelete(DeleteBehavior.Cascade);
        
        // Indexes
        builder.HasIndex(rp => rp.GrantedAt);
        builder.HasIndex(rp => rp.IsActive);
        
        // Seed data
        builder.HasData(SeedRolePermissions());
    }
    
    private static RolePermission[] SeedRolePermissions()
    {
        var rolePermissions = new List<RolePermission>();
        var grantedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        const int grantedBy = 1; // Admin user
        
        // Admin Role - Gets all permissions (1-18)
        for (int permissionId = 1; permissionId <= 18; permissionId++)
        {
            rolePermissions.Add(new RolePermission
            {
                RoleId = 1, // Admin role
                PermissionId = permissionId,
                GrantedAt = grantedAt,
                GrantedBy = grantedBy,
                IsActive = true
            });
        }
        
        // Employee Role - Gets user management and view permissions
        var employeePermissions = new[] { 1, 5, 6, 7, 8, 13, 17, 18 }; // View users, profile management, view roles/permissions, auth
        foreach (var permissionId in employeePermissions)
        {
            rolePermissions.Add(new RolePermission
            {
                RoleId = 2, // Employee role
                PermissionId = permissionId,
                GrantedAt = grantedAt,
                GrantedBy = grantedBy,
                IsActive = true
            });
        }
        
        // User Role - Gets basic profile and auth permissions
        var userPermissions = new[] { 5, 6, 7, 17, 18, 19, 20 }; // Profile management and auth
        foreach (var permissionId in userPermissions)
        {
            rolePermissions.Add(new RolePermission
            {
                RoleId = 3, // User role
                PermissionId = permissionId,
                GrantedAt = grantedAt,
                GrantedBy = grantedBy,
                IsActive = true
            });
        }
        
        return rolePermissions.ToArray();
    }
}
