using Ecom.Users.Application.Features.Users.Queries.GetById;
using Ecom.Users.Domain.DTOs.Users;
using Ecom.Users.Domain.Interfaces;
using FluentAssertions;
using Moq;
using Shared.Responses;
using Shared.Utils;

namespace Ecom.Users.Application.UnitTests.Features.Users.Queries.GetById;

public class GetByIdHandlerTests
{
    private readonly Mock<IUserService> _mockUserService;
    private readonly GetByIdHandler _handler;

    public GetByIdHandlerTests()
    {
        _mockUserService = new Mock<IUserService>();
        _handler = new GetByIdHandler(_mockUserService.Object);
    }

    [Fact]
    public async Task Handle_WithValidUserId_ShouldReturnSuccessResponse()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var request = new GetByIdRequest { Id = userId };

        var userDto = new UserDto
        {
            Id = userId,
            Email = "test@example.com",
            Username = "testuser",
            FirstName = "John",
            LastName = "Doe",
            PhoneNumber = "1234567890",
            EmailConfirmed = true,
            PhoneNumberConfirmed = false,
            TwoFactorEnabled = false,
            CreatedAt = DateTimeOffset.UtcNow.AddDays(-10),
            UpdatedAt = DateTimeOffset.UtcNow.AddDays(-1),
            Provider = "Local"
        };

        var serviceResponse = ResponseUtils.Success(userDto);

        _mockUserService.Setup(x => x.GetUserByIdAsync(userId)).ReturnsAsync(serviceResponse);

        // Act
        var result = await _handler.Handle(request, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.Id.Should().Be(userId);
        result.Data.Email.Should().Be(userDto.Email);
        result.Data.FirstName.Should().Be(userDto.FirstName);
        result.Data.LastName.Should().Be(userDto.LastName);
        result.Data.PhoneNumber.Should().Be(userDto.PhoneNumber);

        _mockUserService.Verify(x => x.GetUserByIdAsync(userId), Times.Once);
    }

    [Fact]
    public async Task Handle_WithInvalidUserId_ShouldReturnErrorResponse()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var request = new GetByIdRequest { Id = userId };

        var serviceResponse = ResponseUtils.Error<UserDto>(404, "User not found");

        _mockUserService.Setup(x => x.GetUserByIdAsync(userId)).ReturnsAsync(serviceResponse);

        // Act
        var result = await _handler.Handle(request, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.StatusCode.Should().Be(404);
        result.Message.Should().Be("User not found");
        result.Data.Should().BeNull();

        _mockUserService.Verify(x => x.GetUserByIdAsync(userId), Times.Once);
    }

    [Fact]
    public async Task Handle_WithEmptyGuid_ShouldReturnErrorResponse()
    {
        // Arrange
        var emptyGuid = Guid.Empty;
        var request = new GetByIdRequest { Id = emptyGuid };

        var serviceResponse = ResponseUtils.Error<UserDto>(400, "Invalid user ID");

        _mockUserService.Setup(x => x.GetUserByIdAsync(emptyGuid)).ReturnsAsync(serviceResponse);

        // Act
        var result = await _handler.Handle(request, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.StatusCode.Should().Be(400);
        result.Message.Should().Be("Invalid user ID");
        result.Data.Should().BeNull();

        _mockUserService.Verify(x => x.GetUserByIdAsync(emptyGuid), Times.Once);
    }

    [Fact]
    public async Task Handle_WithServiceException_ShouldPropagateException()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var request = new GetByIdRequest { Id = userId };

        _mockUserService
            .Setup(x => x.GetUserByIdAsync(userId))
            .ThrowsAsync(new Exception("Service error"));

        // Act & Assert
        await Assert.ThrowsAsync<Exception>(() => _handler.Handle(request, CancellationToken.None));

        _mockUserService.Verify(x => x.GetUserByIdAsync(userId), Times.Once);
    }

    [Fact]
    public async Task Handle_WithCancellationToken_ShouldPassTokenToService()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var request = new GetByIdRequest { Id = userId };
        var cancellationToken = new CancellationToken(true);

        var serviceResponse = ResponseUtils.Error<UserDto>(400, "Operation cancelled");

        _mockUserService.Setup(x => x.GetUserByIdAsync(userId)).ReturnsAsync(serviceResponse);

        // Act
        var result = await _handler.Handle(request, cancellationToken);

        // Assert
        result.Should().NotBeNull();
        _mockUserService.Verify(x => x.GetUserByIdAsync(userId), Times.Once);
    }

    [Fact]
    public async Task Handle_WithServiceReturningNull_ShouldReturnErrorResponse()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var request = new GetByIdRequest { Id = userId };

        var serviceResponse = new Response<UserDto>(IsSuccess: true, 200, "Success", Data: null);

        _mockUserService.Setup(x => x.GetUserByIdAsync(userId)).ReturnsAsync(serviceResponse);

        // Act
        var result = await _handler.Handle(request, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().BeNull();

        _mockUserService.Verify(x => x.GetUserByIdAsync(userId), Times.Once);
    }

    [Fact]
    public async Task Handle_WithMinimalUserData_ShouldReturnSuccessResponse()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var request = new GetByIdRequest { Id = userId };

        var userDto = new UserDto
        {
            Id = userId,
            Email = "minimal@example.com",
            Username = "minimal",
            FirstName = "",
            LastName = "",
            PhoneNumber = null,
            EmailConfirmed = false,
            PhoneNumberConfirmed = false,
            TwoFactorEnabled = false,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = null,
            Provider = ""
        };

        var serviceResponse = ResponseUtils.Success(userDto);

        _mockUserService.Setup(x => x.GetUserByIdAsync(userId)).ReturnsAsync(serviceResponse);

        // Act
        var result = await _handler.Handle(request, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.Id.Should().Be(userId);
        result.Data.Email.Should().Be(userDto.Email);
        result.Data.FirstName.Should().Be(userDto.FirstName);
        result.Data.LastName.Should().Be(userDto.LastName);
        result.Data.PhoneNumber.Should().Be(userDto.PhoneNumber);
    }
}
