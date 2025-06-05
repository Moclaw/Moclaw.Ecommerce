using Domain.Builders;
using Ecom.Users.Application.Services;
using Ecom.Users.Domain.DTOs;
using Ecom.Users.Domain.DTOs.Users;
using Ecom.Users.Domain.Entities;
using Ecom.Users.Domain.Interfaces;
using EfCore.IRepositories;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Shared.Utils;
using System.Linq.Expressions;
using System.Security.Claims;

namespace Ecom.Users.Application.UnitTests.Services;

public class AuthServiceTests
{
    private readonly Mock<ICommandRepository> _mockCommandRepository;
    private readonly Mock<IQueryRepository<User, Guid>> _mockUserRepository;
    private readonly Mock<IQueryRepository<Role, Guid>> _mockRoleRepository;
    private readonly Mock<IQueryRepository<RefreshToken, Guid>> _mockRefreshTokenRepository;
    private readonly Mock<IJwtService> _mockJwtService;
    private readonly Mock<IPasswordHasher> _mockPasswordHasher;
    private readonly Mock<ILogger<AuthService>> _mockLogger;
    private readonly AuthService _authService;

    public AuthServiceTests()
    {
        _mockCommandRepository = new Mock<ICommandRepository>();
        _mockUserRepository = new Mock<IQueryRepository<User, Guid>>();
        _mockRoleRepository = new Mock<IQueryRepository<Role, Guid>>();
        _mockRefreshTokenRepository = new Mock<IQueryRepository<RefreshToken, Guid>>();
        _mockJwtService = new Mock<IJwtService>();
        _mockPasswordHasher = new Mock<IPasswordHasher>();
        _mockLogger = new Mock<ILogger<AuthService>>();

        _authService = new AuthService(
            _mockCommandRepository.Object,
            _mockUserRepository.Object,
            _mockRoleRepository.Object,
            _mockRefreshTokenRepository.Object,
            _mockJwtService.Object,
            _mockPasswordHasher.Object,
            _mockLogger.Object
        );
    }

    [Fact]
    public async Task LoginAsync_WithValidCredentials_ShouldReturnSuccessResponse()
    {
        // Arrange
        var loginDto = new LoginDto
        {
            Email = "test@example.com",
            Password = "validPassword123",
            RememberMe = false
        };

        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = "test@example.com",
            PasswordHash = "hashedPassword",
            EmailConfirmed = true,
            LockoutEnabled = false
        };

        var refreshToken = "refreshTokenValue";
        var accessToken = "accessTokenValue";

        _mockUserRepository
            .Setup(
                x =>
                    x.FirstOrDefaultAsync(
                        It.IsAny<Expression<Func<User, bool>>>(),
                        It.IsAny<Action<IFluentBuilder<User>>>(),
                        false,
                        default
                    )
            )
            .ReturnsAsync(user);

        _mockPasswordHasher
            .Setup(x => x.VerifyPassword(loginDto.Password, user.PasswordHash))
            .Returns(true);

        _mockJwtService
            .Setup(x => x.GenerateAccessToken(user, It.IsAny<IEnumerable<string>>()))
            .Returns(accessToken);

        _mockJwtService
            .Setup(x => x.GenerateRefreshToken(user, It.IsAny<string>()))
            .Returns(new RefreshToken { Token = refreshToken });

        _mockCommandRepository
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        var result = await _authService.LoginAsync(loginDto);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.AccessToken.Should().Be(accessToken);
        result.Data.RefreshToken.Should().Be(refreshToken);
        result.Data.Email.Should().Be(user.Email);
        result.Data.Username.Should().Be(user.UserName);
    }

    [Fact]
    public async Task LoginAsync_WithInvalidEmail_ShouldReturnErrorResponse()
    {
        // Arrange
        var loginDto = new LoginDto
        {
            Email = "nonexistent@example.com",
            Password = "password123",
            RememberMe = false
        };

        _mockUserRepository
            .Setup(
                x =>
                    x.FirstOrDefaultAsync(
                        It.IsAny<Expression<Func<User, bool>>>(),
                        It.IsAny<Action<IFluentBuilder<User>>>(),
                        false,
                        default
                    )
            )
            .ReturnsAsync((User?)null);

        // Act
        var result = await _authService.LoginAsync(loginDto);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.StatusCode.Should().Be(401);
        result.Message.Should().Be("Invalid email or password");
        result.Data.Should().BeNull();
    }

    [Fact]
    public async Task LoginAsync_WithInvalidPassword_ShouldReturnErrorResponse()
    {
        // Arrange
        var loginDto = new LoginDto
        {
            Email = "test@example.com",
            Password = "wrongPassword",
            RememberMe = false
        };

        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = "test@example.com",
            PasswordHash = "hashedPassword",
            EmailConfirmed = true,
            LockoutEnabled = false
        };

        _mockUserRepository
            .Setup(
                x =>
                    x.FirstOrDefaultAsync(
                        It.IsAny<Expression<Func<User, bool>>>(),
                        It.IsAny<Action<IFluentBuilder<User>>>(),
                        false,
                        default
                    )
            )
            .ReturnsAsync(user);

        _mockPasswordHasher
            .Setup(x => x.VerifyPassword(loginDto.Password, user.PasswordHash))
            .Returns(false);

        // Act
        var result = await _authService.LoginAsync(loginDto);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.StatusCode.Should().Be(401);
        result.Message.Should().Be("Invalid email or password");
        result.Data.Should().BeNull();
    }

    [Fact]
    public async Task LoginAsync_WithUnconfirmedEmail_ShouldReturnErrorResponse()
    {
        // Arrange
        var loginDto = new LoginDto
        {
            Email = "test@example.com",
            Password = "validPassword123",
            RememberMe = false
        };

        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = "test@example.com",
            PasswordHash = "hashedPassword",
            EmailConfirmed = false,
            LockoutEnabled = false
        };

        _mockUserRepository
            .Setup(
                x =>
                    x.FirstOrDefaultAsync(
                        It.IsAny<Expression<Func<User, bool>>>(),
                        It.IsAny<Action<IFluentBuilder<User>>>(),
                        false,
                        default
                    )
            )
            .ReturnsAsync(user);

        _mockPasswordHasher
            .Setup(x => x.VerifyPassword(loginDto.Password, user.PasswordHash))
            .Returns(true);

        // Act
        var result = await _authService.LoginAsync(loginDto);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.StatusCode.Should().Be(401);
        result.Message
            .Should()
            .Be("Email not confirmed. Please check your email for confirmation instructions.");
        result.Data.Should().BeNull();
    }

    [Fact]
    public async Task RegisterAsync_WithValidData_ShouldReturnSuccessResponse()
    {
        // Arrange
        var registerDto = new RegisterDto
        {
            Email = "newuser@example.com",
            Password = "validPassword123",
            FirstName = "John",
            LastName = "Doe",
            UserName = "johndoe"
        };

        var hashedPassword = "hashedPassword123";

        _mockUserRepository
            .Setup(
                x =>
                    x.AnyAsync(
                        It.IsAny<Expression<Func<User, bool>>>(),
                        It.IsAny<CancellationToken>()
                    )
            )
            .ReturnsAsync(false);

        _mockPasswordHasher
            .Setup(x => x.HashPassword(registerDto.Password))
            .Returns(hashedPassword);

        _mockCommandRepository
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        var result = await _authService.RegisterAsync(registerDto);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.Email.Should().Be(registerDto.Email);
        result.Data.Username.Should().Be(registerDto.UserName);

        _mockCommandRepository.Verify(
            x => x.AddAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()),
            Times.Once
        );
        _mockCommandRepository.Verify(
            x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Once
        );
    }

    [Fact]
    public async Task RegisterAsync_WithExistingEmail_ShouldReturnErrorResponse()
    {
        // Arrange
        var registerDto = new RegisterDto
        {
            Email = "existing@example.com",
            Password = "validPassword123",
            FirstName = "John",
            LastName = "Doe",
            UserName = "johndoe"
        };

        _mockUserRepository
            .Setup(
                x =>
                    x.AnyAsync(
                        It.IsAny<Expression<Func<User, bool>>>(),
                        It.IsAny<CancellationToken>()
                    )
            )
            .ReturnsAsync(true);

        // Act
        var result = await _authService.RegisterAsync(registerDto);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.StatusCode.Should().Be(400);
        result.Message.Should().Be("User with this email already exists");
        result.Data.Should().BeNull();

        _mockCommandRepository.Verify(
            x => x.AddAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()),
            Times.Never
        );
        _mockCommandRepository.Verify(
            x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Never
        );
    }

    [Fact]
    public async Task RefreshTokenAsync_WithValidToken_ShouldReturnSuccessResponse()
    {
        // Arrange
        var refreshTokenValue = "validRefreshToken";
        var userId = Guid.NewGuid();
        var newAccessToken = "newAccessToken";
        var newRefreshToken = "newRefreshToken";
        var refreshTokenDto = new RefreshTokenDto { RefreshToken = refreshTokenValue };

        var user = new User
        {
            Id = userId,
            Email = "test@example.com",
            EmailConfirmed = true
        };

        var storedRefreshToken = new RefreshToken
        {
            Id = Guid.NewGuid(),
            Token = refreshTokenValue,
            UserId = userId,
            ExpiryDate = DateTimeOffset.UtcNow.AddDays(1),
            IsRevoked = false,
            IsUsed = false,
            User = user
        };

        _mockRefreshTokenRepository
            .Setup(
                x =>
                    x.FirstOrDefaultAsync(
                        It.IsAny<Expression<Func<RefreshToken, bool>>>(),
                        It.IsAny<Action<IFluentBuilder<RefreshToken>>>(),
                        false,
                        default
                    )
            )
            .ReturnsAsync(storedRefreshToken);

        _mockJwtService
            .Setup(x => x.GenerateAccessToken(user, It.IsAny<IEnumerable<string>>()))
            .Returns(newAccessToken);

        _mockJwtService
            .Setup(x => x.GenerateRefreshToken(user, It.IsAny<string>()))
            .Returns(new RefreshToken { Token = newRefreshToken });

        _mockCommandRepository
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        var result = await _authService.RefreshTokenAsync(refreshTokenDto);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.AccessToken.Should().Be(newAccessToken);
        result.Data.RefreshToken.Should().Be(newRefreshToken);
        result.Data.Email.Should().Be(user.Email);
        result.Data.Username.Should().Be(user.UserName);

        storedRefreshToken.IsUsed.Should().BeTrue();
    }

    [Fact]
    public async Task RefreshTokenAsync_WithInvalidToken_ShouldReturnErrorResponse()
    {
        // Arrange
        var refreshTokenValue = "invalidRefreshToken";
        var refreshTokenDto = new RefreshTokenDto { RefreshToken = refreshTokenValue };

        _mockRefreshTokenRepository
            .Setup(
                x =>
                    x.FirstOrDefaultAsync(
                        It.IsAny<Expression<Func<RefreshToken, bool>>>(),
                        It.IsAny<Action<IFluentBuilder<RefreshToken>>>(),
                        false,
                        default
                    )
            )
            .ReturnsAsync((RefreshToken?)null);

        // Act
        var result = await _authService.RefreshTokenAsync(refreshTokenDto);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.StatusCode.Should().Be(401);
        result.Message.Should().Be("Invalid refresh token");
        result.Data.Should().BeNull();
    }

    [Fact]
    public async Task RefreshTokenAsync_WithExpiredToken_ShouldReturnErrorResponse()
    {
        // Arrange
        var refreshTokenValue = "expiredRefreshToken";
        var userId = Guid.NewGuid();
        var refreshTokenDto = new RefreshTokenDto { RefreshToken = refreshTokenValue };

        var user = new User
        {
            Id = userId,
            Email = "test@example.com",
            EmailConfirmed = true
        };

        var expiredRefreshToken = new RefreshToken
        {
            Id = Guid.NewGuid(),
            Token = refreshTokenValue,
            UserId = userId,
            ExpiryDate = DateTimeOffset.UtcNow.AddDays(-1), // Expired
            IsRevoked = false,
            IsUsed = false,
            User = user
        };

        _mockRefreshTokenRepository
            .Setup(
                x =>
                    x.FirstOrDefaultAsync(
                        It.IsAny<Expression<Func<RefreshToken, bool>>>(),
                        It.IsAny<Action<IFluentBuilder<RefreshToken>>>(),
                        false,
                        default
                    )
            )
            .ReturnsAsync(expiredRefreshToken);

        // Act
        var result = await _authService.RefreshTokenAsync(refreshTokenDto);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.StatusCode.Should().Be(401);
        result.Message.Should().Be("Refresh token has expired");
        result.Data.Should().BeNull();
    }
}
