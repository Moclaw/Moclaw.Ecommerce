using Ecom.Users.Domain.Entities;
using FluentAssertions;

namespace Ecom.Users.Domain.UnitTests.Entities;

public class PermissionTests
{
    [Fact]
    public void Permission_Creation_Should_Set_Properties_Correctly()
    {
        // Arrange
        var permissionId = Guid.NewGuid();
        var module = "Users";
        var action = "Read";
        var resource = "User";
        var description = "Read user data";

        // Act
        var permission = new Permission
        {
            Id = permissionId,
            Module = module,
            Action = action,
            Resource = resource,
            Description = description,
            CreatedAt = DateTimeOffset.UtcNow,
            CreatedBy = Guid.NewGuid()
        };

        // Assert
        permission.Id.Should().Be(permissionId);
        permission.Module.Should().Be(module);
        permission.Action.Should().Be(action);
        permission.Resource.Should().Be(resource);
        permission.Description.Should().Be(description);
        permission.RolePermissions.Should().NotBeNull();
    }

    [Fact]
    public void Permission_Default_Values_Should_Be_Set_Correctly()
    {
        // Act
        var permission = new Permission();

        // Assert
        permission.RolePermissions.Should().NotBeNull().And.BeEmpty();
    }    [Theory]
    [InlineData("Users", "Read", "", "Users.Read")]
    [InlineData("Users", "Read", "User", "Users.Read.User")]
    [InlineData("Orders", "Create", "Order", "Orders.Create.Order")]
    [InlineData("Products", "Delete", "", "Products.Delete")]
    public void Permission_FullPermission_Should_Generate_Correctly(string module, string action, string? resource, string expected)
    {
        // Arrange
        var permission = new Permission
        {
            Module = module,
            Action = action,
            Resource = resource
        };

        // Act
        var fullPermission = permission.FullPermission;

        // Assert
        fullPermission.Should().Be(expected);
    }

    [Fact]
    public void Permission_Should_Support_Navigation_Properties()
    {
        // Arrange
        var permission = new Permission 
        { 
            Id = Guid.NewGuid(), 
            Module = "Users", 
            Action = "Read",
            CreatedAt = DateTimeOffset.UtcNow,
            CreatedBy = Guid.NewGuid()
        };
        var role = new Role { Id = Guid.NewGuid(), Name = "TestRole" };

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
        permission.RolePermissions.Add(rolePermission);

        // Assert
        permission.RolePermissions.Should().HaveCount(1);
        permission.RolePermissions.First().Should().Be(rolePermission);
    }
}
