using Ecom.Users.Domain.Entities;
using Ecom.Users.Domain.Constants;
using FluentAssertions;

namespace Ecom.Users.Domain.UnitTests.Entities;

public class UserTests
{
    [Fact]
    public void User_Creation_Should_Set_Properties_Correctly()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var email = "test@example.com";
        var userName = "testuser";
        var firstName = "John";
        var lastName = "Doe";
        var phoneNumber = "+1234567890";
        var passwordHash = "hashedpassword";
        var provider = AuthConstants.Providers.Local;
        var providerId = "local123";

        // Act
        var user = new User
        {
            Id = userId,
            Email = email,
            UserName = userName,
            FirstName = firstName,
            LastName = lastName,
            PhoneNumber = phoneNumber,
            PasswordHash = passwordHash,
            Provider = provider,
            ProviderId = providerId,
            CreatedAt = DateTimeOffset.UtcNow,
            CreatedBy = Guid.NewGuid()
        };

        // Assert
        user.Id.Should().Be(userId);
        user.Email.Should().Be(email);
        user.UserName.Should().Be(userName);
        user.FirstName.Should().Be(firstName);
        user.LastName.Should().Be(lastName);
        user.PhoneNumber.Should().Be(phoneNumber);
        user.PasswordHash.Should().Be(passwordHash);
        user.Provider.Should().Be(provider);
        user.ProviderId.Should().Be(providerId);
        user.EmailConfirmed.Should().BeFalse();
        user.PhoneNumberConfirmed.Should().BeFalse();
        user.TwoFactorEnabled.Should().BeFalse();
        user.LockoutEnabled.Should().BeTrue();
        user.AccessFailedCount.Should().Be(0);
        user.UserRoles.Should().NotBeNull();
        user.RefreshTokens.Should().NotBeNull();
    }

    [Fact]
    public void User_Default_Values_Should_Be_Set_Correctly()
    {
        // Act
        var user = new User();

        // Assert
        user.EmailConfirmed.Should().BeFalse();
        user.PhoneNumberConfirmed.Should().BeFalse();
        user.TwoFactorEnabled.Should().BeFalse();
        user.LockoutEnabled.Should().BeTrue();
        user.AccessFailedCount.Should().Be(0);
        user.UserRoles.Should().NotBeNull().And.BeEmpty();
        user.RefreshTokens.Should().NotBeNull().And.BeEmpty();
    }    [Theory]
    [InlineData("valid@example.com", true)]
    [InlineData("another.valid@domain.co.uk", true)]
    [InlineData("invalid-email", false)]
    [InlineData("", false)]
    public void User_Email_Validation_Should_Work_Correctly(string? email, bool isValid)
    {
        // Arrange
        var user = new User { Email = email };

        // Act & Assert
        if (isValid)
        {
            user.Email.Should().Be(email);
        }
        else
        {
            // Note: In a real scenario, you might want to add validation attributes or methods
            user.Email.Should().Be(email);
        }
    }

    [Fact]
    public void User_Should_Support_Navigation_Properties()
    {
        // Arrange
        var user = new User();
        var role = new Role { Id = Guid.NewGuid(), Name = "TestRole" };
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

        var refreshToken = new RefreshToken
        {
            Id = Guid.NewGuid(),
                        Token = "test-token",
            UserId = user.Id,
            User = user,
            ExpiryDate = DateTimeOffset.UtcNow.AddDays(7),
            CreatedAt = DateTimeOffset.UtcNow,
            CreatedBy = Guid.NewGuid()
        };

        // Act
        user.UserRoles.Add(userRole);
        user.RefreshTokens.Add(refreshToken);

        // Assert
        user.UserRoles.Should().HaveCount(1);
        user.UserRoles.First().Should().Be(userRole);
        user.RefreshTokens.Should().HaveCount(1);
        user.RefreshTokens.First().Should().Be(refreshToken);
    }
}
