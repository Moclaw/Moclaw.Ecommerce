using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Ecom.Users.Infrastructure.Persistence.EfCore.Configurations;

public class PermissionConfiguration : IEntityTypeConfiguration<Permission>
{
    public void Configure(EntityTypeBuilder<Permission> builder)
    {
        builder.ToTable("Permissions", "users");
        
        builder.HasKey(p => p.Id);
        
        builder.Property(p => p.Id)
            .ValueGeneratedNever();
            
        builder.Property(p => p.Name)
            .IsRequired()
            .HasMaxLength(100);
            
        builder.Property(p => p.Description)
            .HasMaxLength(255);
            
        builder.Property(p => p.Module)
            .IsRequired()
            .HasMaxLength(50);
            
        builder.Property(p => p.Category)
            .HasMaxLength(50);
            
        builder.HasIndex(p => p.Name)
            .IsUnique();
            
        builder.HasIndex(p => new { p.Module, p.Category });
        
        // Seed data
        builder.HasData(SeedPermissions());
    }
    
    private static Permission[] SeedPermissions()
    {
        var permissions = new List<Permission>();
        var id = 1;
        
        // User Management Permissions
        permissions.AddRange(new[]
        {
            new Permission { Id = id++, Name = AuthConstants.Permissions.Users.View, Description = "View users", Module = "Users", Category = "User Management" },
            new Permission { Id = id++, Name = AuthConstants.Permissions.Users.Create, Description = "Create users", Module = "Users", Category = "User Management" },
            new Permission { Id = id++, Name = AuthConstants.Permissions.Users.Update, Description = "Update users", Module = "Users", Category = "User Management" },
            new Permission { Id = id++, Name = AuthConstants.Permissions.Users.Delete, Description = "Delete users", Module = "Users", Category = "User Management" },
            new Permission { Id = id++, Name = AuthConstants.Permissions.Users.ViewProfile, Description = "View user profile", Module = "Users", Category = "User Management" },
            new Permission { Id = id++, Name = AuthConstants.Permissions.Users.UpdateProfile, Description = "Update user profile", Module = "Users", Category = "User Management" },
            new Permission { Id = id++, Name = AuthConstants.Permissions.Users.ChangePassword, Description = "Change user password", Module = "Users", Category = "User Management" }
        });
        
        // Role Management Permissions
        permissions.AddRange(new[]
        {
            new Permission { Id = id++, Name = AuthConstants.Permissions.Roles.View, Description = "View roles", Module = "Users", Category = "Role Management" },
            new Permission { Id = id++, Name = AuthConstants.Permissions.Roles.Create, Description = "Create roles", Module = "Users", Category = "Role Management" },
            new Permission { Id = id++, Name = AuthConstants.Permissions.Roles.Update, Description = "Update roles", Module = "Users", Category = "Role Management" },
            new Permission { Id = id++, Name = AuthConstants.Permissions.Roles.Delete, Description = "Delete roles", Module = "Users", Category = "Role Management" },
            new Permission { Id = id++, Name = AuthConstants.Permissions.Roles.AssignPermissions, Description = "Assign permissions to roles", Module = "Users", Category = "Role Management" }
        });
        
        // Permission Management Permissions
        permissions.AddRange(new[]
        {
            new Permission { Id = id++, Name = AuthConstants.Permissions.Permissions.View, Description = "View permissions", Module = "Users", Category = "Permission Management" },
            new Permission { Id = id++, Name = AuthConstants.Permissions.Permissions.Create, Description = "Create permissions", Module = "Users", Category = "Permission Management" },
            new Permission { Id = id++, Name = AuthConstants.Permissions.Permissions.Update, Description = "Update permissions", Module = "Users", Category = "Permission Management" },
            new Permission { Id = id++, Name = AuthConstants.Permissions.Permissions.Delete, Description = "Delete permissions", Module = "Users", Category = "Permission Management" }
        });
        
        // Authentication Permissions
        permissions.AddRange(new[]
        {
            new Permission { Id = id++, Name = AuthConstants.Permissions.Auth.Login, Description = "Login to system", Module = "Users", Category = "Authentication" },
            new Permission { Id = id++, Name = AuthConstants.Permissions.Auth.Register, Description = "Register new account", Module = "Users", Category = "Authentication" },
            new Permission { Id = id++, Name = AuthConstants.Permissions.Auth.RefreshToken, Description = "Refresh authentication token", Module = "Users", Category = "Authentication" },
            new Permission { Id = id++, Name = AuthConstants.Permissions.Auth.SocialLogin, Description = "Login with social media", Module = "Users", Category = "Authentication" }
        });
        
        return permissions.ToArray();
    }
}
