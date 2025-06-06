using Ecom.Users.Application.Features.Auth.Commands.Login;
using Ecom.Users.Domain.Constants;
using Ecom.Users.Domain.DTOs;
using Ecom.Users.Domain.DTOs.Users;
using Ecom.Users.Domain.Interfaces;
using FluentAssertions;
using Moq;
using Shared.Responses;
using Shared.Utils;

namespace Ecom.Users.Application.UnitTests.Features.Auth.Commands.Login;

public class LoginHandlerTests
{
    private readonly Mock<IAuthService> _mockAuthService;
    private readonly LoginHandler _handler;

    public LoginHandlerTests()
    {
        _mockAuthService = new Mock<IAuthService>();
        _handler = new LoginHandler(_mockAuthService.Object);
    }

    [Fact]
    public async Task Handle_WithValidRequest_ShouldReturnSuccessResponse()
    {
        // Arrange
        var request = new LoginRequest
        {
            Email = "test@example.com",
            Password = "validPassword123",
            RememberMe = true
        };

        var authResponse = new AuthResponseDto
        {
            AccessToken = "access_token_value",
            RefreshToken = "refresh_token_value",
            Email = request.Email,
            ExpiresAt = DateTimeOffset.UtcNow.AddHours(1),
            FirstName = "John",
            LastName = "Doe",
            UserId = Guid.NewGuid(),
            Permissions = ["Permission1", "Permission2"],
            Roles = ["Role1", "Role2"],
            UserName = "johndoe"
        };

        var serviceResponse = ResponseUtils.Success(authResponse, MessageKeys.Success);

        _mockAuthService
            .Setup(x => x.LoginAsync(It.IsAny<LoginDto>()))
            .ReturnsAsync(serviceResponse);

        // Act
        var result = await _handler.Handle(request, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Message.Should().Be(MessageKeys.Success);
        result.Data.Should().NotBeNull();
        result.Data!.AccessToken.Should().Be(authResponse.AccessToken);
        result.Data.RefreshToken.Should().Be(authResponse.RefreshToken);
        result.Data.Email.Should().Be(authResponse.Email);
        result.Data.UserId.Should().Be(authResponse.UserId);

        // Verify the service was called with correct parameters
        _mockAuthService.Verify(
            x =>
                x.LoginAsync(
                    It.Is<LoginDto>(
                        dto =>
                            dto.Email == request.Email
                            && dto.Password == request.Password
                            && dto.RememberMe == request.RememberMe
                    )
                ),
            Times.Once
        );
    }

    [Fact]
    public async Task Handle_WithInvalidCredentials_ShouldReturnErrorResponse()
    {
        // Arrange
        var request = new LoginRequest
        {
            Email = "test@example.com",
            Password = "invalidPassword",
            RememberMe = false
        };

        var serviceResponse = ResponseUtils.Error<AuthResponseDto>(
            400,
            MessageKeys.InvalidCredentials
        );

        _mockAuthService
            .Setup(x => x.LoginAsync(It.IsAny<LoginDto>()))
            .ReturnsAsync(serviceResponse);

        // Act
        var result = await _handler.Handle(request, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.StatusCode.Should().Be(400); 
        result.Message.Should().Be(MessageKeys.InvalidCredentials);
        result.Data.Should().BeNull();

        _mockAuthService.Verify(x => x.LoginAsync(It.IsAny<LoginDto>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WithServiceException_ShouldThrow()
    {
        // Arrange
        var request = new LoginRequest
        {
            Email = "test@example.com",
            Password = "password123",
            RememberMe = false
        };

        _mockAuthService
            .Setup(x => x.LoginAsync(It.IsAny<LoginDto>()))
            .ThrowsAsync(new Exception("Service error"));

        // Act & Assert
        await Assert.ThrowsAsync<Exception>(() => _handler.Handle(request, CancellationToken.None));

        _mockAuthService.Verify(x => x.LoginAsync(It.IsAny<LoginDto>()), Times.Once);
    }

    [Theory]
    [InlineData("", "password123", false)]
    [InlineData(null, "password123", false)]
    [InlineData("test@example.com", "", true)]
    [InlineData("test@example.com", null, true)]
    public async Task Handle_WithInvalidRequestData_ShouldStillCallService(
        string? email,
        string? password,
        bool rememberMe
    )
    {
        // Arrange
        var request = new LoginRequest
        {
            Email = email ?? string.Empty,
            Password = password ?? string.Empty,
            RememberMe = rememberMe
        };

        var serviceResponse = ResponseUtils.Error<AuthResponseDto>(400, "Invalid input");

        _mockAuthService
            .Setup(x => x.LoginAsync(It.IsAny<LoginDto>()))
            .ReturnsAsync(serviceResponse);

        // Act
        var result = await _handler.Handle(request, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();

        _mockAuthService.Verify(
            x =>
                x.LoginAsync(
                    It.Is<LoginDto>(
                        dto =>
                            dto.Email == (email ?? string.Empty)
                            && dto.Password == (password ?? string.Empty)
                            && dto.RememberMe == rememberMe
                    )
                ),
            Times.Once
        );
    }

    [Fact]
    public async Task Handle_WithCancellationToken_ShouldPassTokenToService()
    {
        // Arrange
        var request = new LoginRequest
        {
            Email = "test@example.com",
            Password = "password123",
            RememberMe = false
        };

        var cancellationToken = new CancellationToken(true);
        var serviceResponse = ResponseUtils.Error<AuthResponseDto>(400, "Operation cancelled");

        _mockAuthService
            .Setup(x => x.LoginAsync(It.IsAny<LoginDto>()))
            .ReturnsAsync(serviceResponse);

        // Act
        var result = await _handler.Handle(request, cancellationToken);

        // Assert
        result.Should().NotBeNull();
        _mockAuthService.Verify(x => x.LoginAsync(It.IsAny<LoginDto>()), Times.Once);
    }
}
