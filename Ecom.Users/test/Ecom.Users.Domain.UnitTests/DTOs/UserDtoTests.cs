using Ecom.Users.Domain.DTOs.Users;
using FluentAssertions;

namespace Ecom.Users.Domain.UnitTests.DTOs;

public class UserDtoTests
{
    [Fact]
    public void UserDto_Creation_Should_Set_Properties_Correctly()
    {
        // Arrange
        var id = Guid.NewGuid();
        var email = "test@example.com";
        var username = "testuser";
        var firstName = "John";
        var lastName = "Doe";
        var phoneNumber = "+1234567890";
        var provider = "Local";
        var createdAt = DateTimeOffset.UtcNow;
        var updatedAt = DateTimeOffset.UtcNow;

        // Act
        var userDto = new UserDto
        {
            Id = id,
            Email = email,
            Username = username,
            FirstName = firstName,
            LastName = lastName,
            PhoneNumber = phoneNumber,
            EmailConfirmed = true,
            PhoneNumberConfirmed = true,
            TwoFactorEnabled = false,
            CreatedAt = createdAt,
            UpdatedAt = updatedAt,
            Provider = provider
        };

        // Assert
        userDto.Id.Should().Be(id);
        userDto.Email.Should().Be(email);
        userDto.Username.Should().Be(username);
        userDto.FirstName.Should().Be(firstName);
        userDto.LastName.Should().Be(lastName);
        userDto.PhoneNumber.Should().Be(phoneNumber);
        userDto.EmailConfirmed.Should().BeTrue();
        userDto.PhoneNumberConfirmed.Should().BeTrue();
        userDto.TwoFactorEnabled.Should().BeFalse();
        userDto.CreatedAt.Should().Be(createdAt);
        userDto.UpdatedAt.Should().Be(updatedAt);
        userDto.Provider.Should().Be(provider);
    }

    [Fact]
    public void EditUserDto_Creation_Should_Set_Properties_Correctly()
    {
        // Arrange
        var firstName = "John";
        var lastName = "Doe";
        var username = "johndoe";
        var phoneNumber = "+1234567890";

        // Act
        var editUserDto = new EditUserDto
        {
            FirstName = firstName,
            LastName = lastName,
            Username = username,
            PhoneNumber = phoneNumber
        };

        // Assert
        editUserDto.FirstName.Should().Be(firstName);
        editUserDto.LastName.Should().Be(lastName);
        editUserDto.Username.Should().Be(username);
        editUserDto.PhoneNumber.Should().Be(phoneNumber);
    }

    [Fact]
    public void UpdatePasswordDto_Creation_Should_Set_Properties_Correctly()
    {
        // Arrange
        var currentPassword = "currentPassword123";
        var newPassword = "newPassword123";
        var confirmNewPassword = "newPassword123";

        // Act
        var updatePasswordDto = new UpdatePasswordDto
        {
            CurrentPassword = currentPassword,
            NewPassword = newPassword,
            ConfirmNewPassword = confirmNewPassword
        };

        // Assert
        updatePasswordDto.CurrentPassword.Should().Be(currentPassword);
        updatePasswordDto.NewPassword.Should().Be(newPassword);
        updatePasswordDto.ConfirmNewPassword.Should().Be(confirmNewPassword);
    }

    [Fact]
    public void RoleDto_Creation_Should_Set_Properties_Correctly()
    {
        // Arrange
        var id = Guid.NewGuid();
        var name = "Admin";
        var normalizedName = "ADMIN";
        var description = "Administrator role";

        // Act
        var roleDto = new RoleDto
        {
            Id = id,
            Name = name,
            NormalizedName = normalizedName,
            Description = description
        };

        // Assert
        roleDto.Id.Should().Be(id);
        roleDto.Name.Should().Be(name);
        roleDto.NormalizedName.Should().Be(normalizedName);
        roleDto.Description.Should().Be(description);
    }

    [Fact]
    public void PermissionDto_Creation_Should_Set_Properties_Correctly()
    {
        // Arrange
        var id = Guid.NewGuid();
        var module = "Users";
        var action = "Read";
        var resource = "User";
        var description = "Read user data";

        // Act
        var permissionDto = new PermissionDto
        {
            Id = id,
            Module = module,
            Action = action,
            Resource = resource,
            Description = description
        };

        // Assert
        permissionDto.Id.Should().Be(id);
        permissionDto.Module.Should().Be(module);
        permissionDto.Action.Should().Be(action);
        permissionDto.Resource.Should().Be(resource);
        permissionDto.Description.Should().Be(description);
    }    [Theory]
    [InlineData("Users", "Read", "", "Users.Read")]
    [InlineData("Users", "Read", "User", "Users.Read.User")]
    [InlineData("Orders", "Create", "Order", "Orders.Create.Order")]
    [InlineData("Products", "Delete", "", "Products.Delete")]
    public void PermissionDto_FullPermission_Should_Generate_Correctly(string module, string action, string? resource, string expected)
    {
        // Arrange
        var permissionDto = new PermissionDto
        {
            Module = module,
            Action = action,
            Resource = resource
        };

        // Act
        var fullPermission = permissionDto.FullPermission;

        // Assert
        fullPermission.Should().Be(expected);
    }
}
