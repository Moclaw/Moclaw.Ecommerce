using Ecom.Users.Domain.Entities;
using FluentAssertions;

namespace Ecom.Users.Domain.UnitTests.Entities;

public class RefreshTokenTests
{
    [Fact]
    public void RefreshToken_Creation_Should_Set_Properties_Correctly()
    {
        // Arrange
        var tokenId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var token = "refresh-token-value";
        var jwtId = "jwt-id-value"; 
        var expiryDate = DateTimeOffset.UtcNow.AddDays(7);

        // Act
        var refreshToken = new RefreshToken
        {
            Id = tokenId,
            Token = token,
            JwtId = jwtId,
            UserId = userId,
            ExpiryDate = expiryDate,
            CreatedAt = DateTimeOffset.UtcNow,
            CreatedBy = userId
        };

        // Assert
        refreshToken.Id.Should().Be(tokenId);
        refreshToken.Token.Should().Be(token);
        refreshToken.JwtId.Should().Be(jwtId);
        refreshToken.UserId.Should().Be(userId);
        refreshToken.ExpiryDate.Should().Be(expiryDate);
        refreshToken.IsRevoked.Should().BeFalse();
        refreshToken.IsUsed.Should().BeFalse();
    }

    [Fact]
    public void RefreshToken_Default_Values_Should_Be_Set_Correctly()
    {
        // Act
        var refreshToken = new RefreshToken();

        // Assert
        refreshToken.IsRevoked.Should().BeFalse();
        refreshToken.IsUsed.Should().BeFalse();
    }
    [Fact]
    public void RefreshToken_IsExpired_Should_Work_Correctly()
    {
        // Arrange
        var expiredToken = new RefreshToken
        {
            ExpiryDate = DateTimeOffset.UtcNow.AddDays(-1)
        };

        var validToken = new RefreshToken
        {
            ExpiryDate = DateTimeOffset.UtcNow.AddDays(1)
        };

        // Act & Assert
        expiredToken.IsExpired.Should().BeTrue();
        validToken.IsExpired.Should().BeFalse();
    }

    [Fact]
    public void RefreshToken_IsActive_Should_Work_Correctly()
    {
        // Arrange
        var validToken = new RefreshToken
        {
            ExpiryDate = DateTimeOffset.UtcNow.AddDays(1),
            IsRevoked = false,
            IsUsed = false
        };

        var expiredToken = new RefreshToken
        {
            ExpiryDate = DateTimeOffset.UtcNow.AddDays(-1),
            IsRevoked = false,
            IsUsed = false
        };

        var revokedToken = new RefreshToken
        {
            ExpiryDate = DateTimeOffset.UtcNow.AddDays(1),
            IsRevoked = true,
            IsUsed = false
        };

        var usedToken = new RefreshToken
        {
            ExpiryDate = DateTimeOffset.UtcNow.AddDays(1),
            IsRevoked = false,
            IsUsed = true
        };

        // Act & Assert
        validToken.IsActive.Should().BeTrue();
        expiredToken.IsActive.Should().BeFalse();
        revokedToken.IsActive.Should().BeFalse();
        usedToken.IsActive.Should().BeFalse();
    }

    [Fact]
    public void RefreshToken_Should_Support_User_Navigation_Property()
    {
        // Arrange
        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = "test@example.com",
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
            CreatedBy = user.Id
        };

        // Act
        user.RefreshTokens.Add(refreshToken);

        // Assert
        refreshToken.User.Should().Be(user);
        user.RefreshTokens.Should().Contain(refreshToken);
    }
}
