using Ecom.Users.Domain.Entities;
using FluentAssertions;

namespace Ecom.Users.Domain.UnitTests.Entities;

public class RoleTests
{
    [Fact]
    public void Role_Creation_Should_Set_Properties_Correctly()
    {
        // Arrange
        var roleId = Guid.NewGuid();
        var name = "Admin";
        var normalizedName = "ADMIN";
        var description = "Administrator role";

        // Act
        var role = new Role
        {
            Id = roleId,
            Name = name,
            NormalizedName = normalizedName,
            Description = description,
            CreatedAt = DateTimeOffset.UtcNow,
            CreatedBy = Guid.NewGuid()
        };

        // Assert
        role.Id.Should().Be(roleId);
        role.Name.Should().Be(name);
        role.NormalizedName.Should().Be(normalizedName);
        role.Description.Should().Be(description);
        role.UserRoles.Should().NotBeNull();
        role.RolePermissions.Should().NotBeNull();
    }

    [Fact]
    public void Role_Default_Values_Should_Be_Set_Correctly()
    {
        // Act
        var role = new Role();

        // Assert
        role.UserRoles.Should().NotBeNull().And.BeEmpty();
        role.RolePermissions.Should().NotBeNull().And.BeEmpty();
    }

    [Fact]
    public void Role_Should_Support_Navigation_Properties()
    {
        // Arrange
        var role = new Role { Id = Guid.NewGuid(), Name = "TestRole" };
        var user = new User { Id = Guid.NewGuid(), Email = "test@example.com" };
        var permission = new Permission 
        { 
            Id = Guid.NewGuid(), 
            Module = "Users", 
            Action = "Read",
            CreatedAt = DateTimeOffset.UtcNow,
            CreatedBy = Guid.NewGuid()
        };

        var userRole = new UserRole 
        { 
            Id = Guid.NewGuid(), 
            UserId = user.Id, 
            RoleId = role.Id, 
            User = user, 
            Role = role,
            CreatedAt = DateTimeOffset.UtcNow,
            CreatedBy = Guid.NewGuid()
        };

        var rolePermission = new RolePermission
        {
            Id = Guid.NewGuid(),
            RoleId = role.Id,
            PermissionId = permission.Id,
            Role = role,
            Permission = permission,
            CreatedAt = DateTimeOffset.UtcNow,
            CreatedBy = Guid.NewGuid()
        };

        // Act
        role.UserRoles.Add(userRole);
        role.RolePermissions.Add(rolePermission);

        // Assert
        role.UserRoles.Should().HaveCount(1);
        role.UserRoles.First().Should().Be(userRole);
        role.RolePermissions.Should().HaveCount(1);
        role.RolePermissions.First().Should().Be(rolePermission);
    }
}
