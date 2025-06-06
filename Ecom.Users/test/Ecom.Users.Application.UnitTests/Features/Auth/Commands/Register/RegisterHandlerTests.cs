using Ecom.Users.Application.Features.Auth.Commands.Register;
using Ecom.Users.Domain.Constants;
using Ecom.Users.Domain.DTOs;
using Ecom.Users.Domain.Interfaces;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.Amqp.Framing;
using Moq;
using Shared.Utils;

namespace Ecom.Users.Application.UnitTests.Features.Auth.Commands.Register
{
    public class RegisterHandlerTests
    {
        private readonly Mock<IAuthService> _mockAuthService;
        private readonly RegisterHandler _handler;

        public RegisterHandlerTests()
        {
            _mockAuthService = new Mock<IAuthService>();
            _handler = new RegisterHandler(_mockAuthService.Object);
        }

        [Fact]
        public async Task Handle_WhenSuccessful_ReturnsSuccessResponse()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var request = new RegisterRequest
            {
                Email = "test@example.com",
                Password = "Password123!",
                ConfirmPassword = "Password123!",
                UserName = "testuser",
                FirstName = "Test",
                LastName = "User"
            };

            var authResponse = new AuthResponseDto
            {
                UserId = userId,
                Email = "test@example.com",
                AccessToken = "access-token",
                RefreshToken = "refresh-token",
                ExpiresAt = DateTimeOffset.UtcNow.AddHours(1),
                Roles = ["User"]
            };

            _mockAuthService
            .Setup(x => x.RegisterAsync(It.IsAny<RegisterDto>()))
            .ReturnsAsync(
                new Shared.Responses.Response<AuthResponseDto>(true, 200, MessageKeys.Success, authResponse));

            // Act
            var result = await _handler.Handle(request, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeTrue();
            result.StatusCode.Should().Be(200);
            result.Data.Should().NotBeNull();
            result.Data!.UserId.Should().Be(userId);
            result.Data.Email.Should().Be("test@example.com");
            result.Data.Roles.Should().ContainSingle("User");
            result.Message.Should().Be(MessageKeys.Success);
        }

        [Fact]
        public async Task Handle_WithEmptyEmail_ReturnsError()
        {
            // Arrange
            var request = new RegisterRequest { Email = "", Password = "Password123!" };

            // Act
            var result = await _handler.Handle(request, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeFalse();
            result.StatusCode.Should().Be(400);
            result.Message.Should().Be("Email is required");
            result.Data.Should().BeNull();
        }

        [Fact]
        public async Task Handle_WithEmptyPassword_ReturnsError()
        {
            // Arrange
            var request = new RegisterRequest { Email = "test@example.com", Password = "" };

            // Act
            var result = await _handler.Handle(request, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeFalse();
            result.StatusCode.Should().Be(400);
            result.Message.Should().Be("Password is required");
            result.Data.Should().BeNull();
        }

        [Fact]
        public async Task Handle_WhenRegistrationFails_ReturnsError()
        {
            // Arrange
            var request = new RegisterRequest
            {
                Email = "existing@example.com",
                Password = "Password123!",
                ConfirmPassword = "Password123!"
            };

            _mockAuthService
                .Setup(x => x.RegisterAsync(It.IsAny<RegisterDto>()))
                .ReturnsAsync(
                    ResponseUtils.Error<AuthResponseDto>(
                        409,
                        MessageKeys.EmailAlreadyExists
                    )
                );

            // Act
            var result = await _handler.Handle(request, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeFalse();
            result.StatusCode.Should().Be(409);
            result.Message.Should().Be(MessageKeys.EmailAlreadyExists);
            result.Data.Should().BeNull();
        }
    }
}
