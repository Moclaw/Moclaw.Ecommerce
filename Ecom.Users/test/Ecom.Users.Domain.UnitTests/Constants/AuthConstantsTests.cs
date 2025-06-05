using Ecom.Users.Domain.Constants;
using FluentAssertions;

namespace Ecom.Users.Domain.UnitTests.Constants;

public class AuthConstantsTests
{
    [Fact]
    public void Roles_Constants_Should_Have_Correct_Values()
    {
        // Assert
        AuthConstants.Roles.Admin.Should().Be("Admin");
        AuthConstants.Roles.Employee.Should().Be("Employee");
        AuthConstants.Roles.User.Should().Be("User");
    }

    [Fact]
    public void Modules_Constants_Should_Have_Correct_Values()
    {
        // Assert
        AuthConstants.Modules.Users.Should().Be("Users");
        AuthConstants.Modules.Roles.Should().Be("Roles");
        AuthConstants.Modules.Permissions.Should().Be("Permissions");
    }

    [Fact]
    public void Actions_Constants_Should_Have_Correct_Values()
    {
        // Assert
        AuthConstants.Actions.Create.Should().Be("Create");
        AuthConstants.Actions.Read.Should().Be("Read");
        AuthConstants.Actions.Update.Should().Be("Update");
        AuthConstants.Actions.Delete.Should().Be("Delete");
        AuthConstants.Actions.List.Should().Be("List");
    }

    [Fact]
    public void Permissions_Constants_Should_Have_Correct_Values()
    {
        // Assert
        AuthConstants.Permissions.UsersRead.Should().Be("Users.Read");
        AuthConstants.Permissions.UsersCreate.Should().Be("Users.Create");
        AuthConstants.Permissions.UsersUpdate.Should().Be("Users.Update");
        AuthConstants.Permissions.UsersDelete.Should().Be("Users.Delete");
        AuthConstants.Permissions.UsersList.Should().Be("Users.List");

        AuthConstants.Permissions.RolesRead.Should().Be("Roles.Read");
        AuthConstants.Permissions.RolesCreate.Should().Be("Roles.Create");
        AuthConstants.Permissions.RolesUpdate.Should().Be("Roles.Update");
        AuthConstants.Permissions.RolesDelete.Should().Be("Roles.Delete");

        AuthConstants.Permissions.PermissionsRead.Should().Be("Permissions.Read");
        AuthConstants.Permissions.PermissionsCreate.Should().Be("Permissions.Create");
        AuthConstants.Permissions.PermissionsUpdate.Should().Be("Permissions.Update");
        AuthConstants.Permissions.PermissionsDelete.Should().Be("Permissions.Delete");
    }

    [Fact]
    public void Providers_Constants_Should_Have_Correct_Values()
    {
        // Assert
        AuthConstants.Providers.Local.Should().Be("Local");
        AuthConstants.Providers.Google.Should().Be("Google");
        AuthConstants.Providers.Facebook.Should().Be("Facebook");
    }

    [Fact]
    public void ClaimTypes_Constants_Should_Have_Correct_Values()
    {
        // Assert
        AuthConstants.ClaimTypes.UserId.Should().Be("user_id");
        AuthConstants.ClaimTypes.Email.Should().Be("email");
        AuthConstants.ClaimTypes.Role.Should().Be("role");
        AuthConstants.ClaimTypes.Permission.Should().Be("permission");
        AuthConstants.ClaimTypes.Provider.Should().Be("provider");
        AuthConstants.ClaimTypes.UserName.Should().Be("username");
    }

    [Fact]
    public void TokenTypes_Constants_Should_Have_Correct_Values()
    {
        // Assert
        AuthConstants.TokenTypes.AccessToken.Should().Be("access_token");
        AuthConstants.TokenTypes.RefreshToken.Should().Be("refresh_token");
    }
}
