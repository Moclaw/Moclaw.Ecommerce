using Ecom.Users.Application.Features.Auth.Commands.UpdateUser;
using Ecom.Users.Domain.Constants;
using Ecom.Users.Domain.DTOs;
using Ecom.Users.Domain.Interfaces;
using FluentAssertions;
using Moq;
using Shared.Utils;

namespace Ecom.Users.Application.UnitTests.Features.Auth.Commands.UpdateUser
{
    public class UpdateUserHandlerTests
    {
        private readonly Mock<IAuthService> _mockAuthService;
        private readonly UpdateUserHandler _handler;

        public UpdateUserHandlerTests()
        {
            _mockAuthService = new Mock<IAuthService>();
            _handler = new UpdateUserHandler(_mockAuthService.Object);
        }

        [Fact]
        public async Task Handle_WhenSuccessful_ReturnsSuccessResponse()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var request = new UpdateUserRequest
            {
                CurrentUserId = userId,
                TargetUserId = userId,
                Email = "test@example.com",
                UserName = "testuser",
                FirstName = "Test",
                LastName = "User"
            };

            _mockAuthService.Setup(x => x.UpdateUserAsync(
                It.IsAny<Guid>(),
                It.IsAny<Guid>(),
                It.IsAny<UpdateUserDto>(),
                It.IsAny<bool>()
            )).ReturnsAsync( new Shared.Responses.Response<bool>(
                true,
                200,
                MessageKeys.Success,
                true
            ));

            // Act
            var result = await _handler.Handle(request, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeTrue();
            result.StatusCode.Should().Be(200);
            result.Data.Should().NotBeNull();
            result.Data!.UserId.Should().Be(userId);
            result.Data.IsSuccess.Should().BeTrue();
            result.Message.Should().Be(MessageKeys.Success);
        }

        [Fact]
        public async Task Handle_WhenFails_ReturnsErrorResponse()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var request = new UpdateUserRequest
            {
                CurrentUserId = userId,
                TargetUserId = userId,
                Email = "test@example.com"
            };

            var errorMessage = MessageKeys.Error;
            _mockAuthService.Setup(x => x.UpdateUserAsync(
                It.IsAny<Guid>(),
                It.IsAny<Guid>(),
                It.IsAny<UpdateUserDto>(),
                It.IsAny<bool>()
            )).ReturnsAsync( new Shared.Responses.Response<bool>(
                false,
                400,
                errorMessage,
                false
            ));

            // Act
            var result = await _handler.Handle(request, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeFalse();
            result.StatusCode.Should().Be(400);
            result.Message.Should().Be(errorMessage);
            result.Data.Should().BeNull();
        }
    }
}
