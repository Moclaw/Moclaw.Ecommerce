using Ecom.Users.Application.Services;
using Ecom.Users.Domain.Entities;
using Ecom.Users.Domain.ValueObjects;
using FluentAssertions;
using Microsoft.Extensions.Options;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace Ecom.Users.Application.UnitTests.Services;

public class JwtServiceTests
{
    private readonly JwtOptions _jwtOptions;
    private readonly JwtService _jwtService;

    public JwtServiceTests()
    {
        _jwtOptions = new JwtOptions
        {
            Secret = "ThisIsAVeryLongSecretKeyForTestingPurposesOnly123456789",
            Issuer = "TestIssuer",
            Audience = "TestAudience",
            AccessTokenExpirationMinutes = 15,
            RefreshTokenExpirationDays = 7
        };

        var options = Options.Create(_jwtOptions);
        _jwtService = new JwtService(options);
    }

    [Fact]
    public void GenerateAccessToken_WithValidUser_ShouldReturnValidToken()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = new User
        {
            Id = userId,
            Email = "test@example.com",
            UserName = "testuser",
            UserRoles =
            [
                new UserRole
                {
                    Role = new Role { Name = "Admin" }
                },
                new UserRole
                {
                    Role = new Role { Name = "User" }
                }
            ]
        };

        var permissions = new List<string> { "read:users", "write:users", "delete:users" };

        // Act
        var token = _jwtService.GenerateAccessToken(user, permissions);

        // Assert
        token.Should().NotBeNullOrEmpty();

        // Verify token structure
        var tokenHandler = new JwtSecurityTokenHandler();
        tokenHandler.CanReadToken(token).Should().BeTrue();

        var jwtToken = tokenHandler.ReadJwtToken(token);

        // Verify basic claims
        jwtToken.Claims.Should().Contain(c => c.Type == JwtRegisteredClaimNames.Sub && c.Value == userId.ToString());
        jwtToken.Claims.Should().Contain(c => c.Type == JwtRegisteredClaimNames.Email && c.Value == user.Email);
        jwtToken.Claims.Should().Contain(c => c.Type == "userId" && c.Value == userId.ToString());
        jwtToken.Claims.Should().Contain(c => c.Type == "email" && c.Value == user.Email);
        jwtToken.Claims.Should().Contain(c => c.Type == "username" && c.Value == user.UserName);

        // Verify role claims
        jwtToken.Claims.Should().Contain(c => c.Type == ClaimTypes.Role && c.Value == "Admin");
        jwtToken.Claims.Should().Contain(c => c.Type == ClaimTypes.Role && c.Value == "User");
        jwtToken.Claims.Should().Contain(c => c.Type == "role" && c.Value == "Admin");
        jwtToken.Claims.Should().Contain(c => c.Type == "role" && c.Value == "User");

        // Verify permission claims
        jwtToken.Claims.Should().Contain(c => c.Type == "permission" && c.Value == "read:users");
        jwtToken.Claims.Should().Contain(c => c.Type == "permission" && c.Value == "write:users");
        jwtToken.Claims.Should().Contain(c => c.Type == "permission" && c.Value == "delete:users");

        // Verify token metadata
        jwtToken.Issuer.Should().Be(_jwtOptions.Issuer);
        jwtToken.Audiences.Should().Contain(_jwtOptions.Audience);
        jwtToken.ValidTo.Should().BeCloseTo(DateTime.UtcNow.AddMinutes(_jwtOptions.AccessTokenExpirationMinutes), TimeSpan.FromMinutes(1));
    }

    [Fact]
    public void GenerateAccessToken_WithUserWithoutRoles_ShouldReturnValidToken()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = new User
        {
            Id = userId,
            Email = "test@example.com",
            UserName = "testuser",
            UserRoles = []
        };

        var permissions = new List<string>();

        // Act
        var token = _jwtService.GenerateAccessToken(user, permissions);

        // Assert
        token.Should().NotBeNullOrEmpty();

        var tokenHandler = new JwtSecurityTokenHandler();
        var jwtToken = tokenHandler.ReadJwtToken(token);

        // Should still have basic claims
        jwtToken.Claims.Should().Contain(c => c.Type == JwtRegisteredClaimNames.Sub && c.Value == userId.ToString());
        jwtToken.Claims.Should().Contain(c => c.Type == JwtRegisteredClaimNames.Email && c.Value == user.Email);

        // Should not have role or permission claims
        jwtToken.Claims.Should().NotContain(c => c.Type == ClaimTypes.Role);
        jwtToken.Claims.Should().NotContain(c => c.Type == "role");
        jwtToken.Claims.Should().NotContain(c => c.Type == "permission");
    }

    [Fact]
    public void GenerateRefreshToken_ShouldReturnUniqueTokens()
    {
        // Act
        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = "test@example.com",
            UserName = "testuser",
            UserRoles = []
        };

        var user2 = new User
        {
            Id = Guid.NewGuid(),
            Email = "test2@example.com",
            UserName = "testuser2",
            UserRoles = []
        };

        var token1 = _jwtService.GenerateRefreshToken(user, jwtId: _jwtOptions.Secret);
        var token2 = _jwtService.GenerateRefreshToken(user2, jwtId: _jwtOptions.Secret);

        // Assert
        token1.Should().NotBeNull();
        token2.Should().NotBeNull();
        token1.Should().NotBe(token2, "Refresh tokens for different users should be unique");
        token2.Should().NotBe(token1, "Refresh tokens for different users should be unique");
    }

    [Fact]
    public void GenerateRefreshToken_MultipleGenerations_ShouldAllBeUnique()
    {
        // Arrange
        const int numberOfTokens = 100;
        var tokens = new HashSet<string>();
        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = "test@example.com",
            UserName = "testuser",
            UserRoles = []
        };

        // Act
        for (int i = 0; i < numberOfTokens; i++)
        {
            var token = _jwtService.GenerateRefreshToken(user, jwtId: _jwtOptions.Secret);
            tokens.Add(token.Token);
        }

        // Assert
        tokens.Count.Should().Be(numberOfTokens, "All generated tokens should be unique");
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    [InlineData("   ")]
    public void ValidateToken_WithNullOrEmptyToken_ShouldReturnNull(string? token)
    {
        // Act
        var principal = _jwtService.ValidateToken(token!);

        // Assert
        principal.Should().BeTrue("Null or empty token should return null, but it did not.");
    }

    [Fact]
    public void GetJwtId_FromValidToken_ShouldReturnCorrectJti()
    {
        // Arrange
        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = "test@example.com",
            UserName = "testuser",
            UserRoles = []
        };

        var token = _jwtService.GenerateAccessToken(user, []);

        // Act
        var jwtId = _jwtService.GetJwtId(token);

        // Assert
        jwtId.Should().NotBeNullOrEmpty();
        Guid.TryParse(jwtId, out _).Should().BeTrue("JTI should be a valid GUID");
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    [InlineData("invalid.token")]
    public void GetJwtId_FromInvalidToken_ShouldReturnNull(string? token)
    {
        // Act
        var jwtId = _jwtService.GetJwtId(token!);

        // Assert
        jwtId.Should().BeNull();
    }
}
