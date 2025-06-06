using Ecom.Users.Application.Features.Auth.Commands.RefreshToken;
using Ecom.Users.Domain.Constants;
using Ecom.Users.Domain.DTOs;
using Ecom.Users.Domain.Interfaces;
using FluentAssertions;
using Moq;
using Shared.Responses;

namespace Ecom.Users.Application.UnitTests.Features.Auth.Commands.RefreshToken
{
    public class RefreshTokenHandlerTests
    {
        private readonly Mock<IAuthService> _mockAuthService;
        private readonly RefreshTokenHandler _handler;

        public RefreshTokenHandlerTests()
        {
            _mockAuthService = new Mock<IAuthService>();
            _handler = new RefreshTokenHandler(_mockAuthService.Object);
        }

        [Fact]
        public async Task Handle_WhenSuccessful_ReturnsSuccessResponse()
        {
            // Arrange
            var request = new RefreshTokenRequest { RefreshToken = "valid-refresh-token" };

            var authDto = new AuthResponseDto
            {
                AccessToken = "new-access-token",
                RefreshToken = "new-refresh-token",
                ExpiresAt = DateTime.UtcNow.AddHours(1)
            };

            _mockAuthService
                .Setup(x => x.RefreshTokenAsync(It.IsAny<RefreshTokenDto>()))
                .ReturnsAsync(
                    new Response<AuthResponseDto>(
                        true,
                        200,
                        MessageKeys.Success,
                        authDto
                    )
                );

            // Act
            var result = await _handler.Handle(request, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeTrue();
            result.StatusCode.Should().Be(200);
            result.Data.Should().NotBeNull();
            result.Data!.AccessToken.Should().Be("new-access-token");
            result.Data.RefreshToken.Should().Be("new-refresh-token");
            result.Message.Should().Be(MessageKeys.Success);
        }

        [Fact]
        public async Task Handle_WhenFails_ReturnsErrorResponse()
        {
            // Arrange
            var request = new RefreshTokenRequest { RefreshToken = "invalid-refresh-token" };

            _mockAuthService
            .Setup(x => x.RefreshTokenAsync(It.IsAny<RefreshTokenDto>()))
            .ReturnsAsync(
                new Response<AuthResponseDto>(
                    false, // IsSuccess
                    401,  // StatusCode
                    MessageKeys.Unauthorized,
                    null  // Data\
                )
            );

            // Act
            var result = await _handler.Handle(request, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeFalse();
            result.StatusCode.Should().Be(401);
            result.Message.Should().Be(MessageKeys.Unauthorized);
            result.Data.Should().BeNull();
        }
    }
}
