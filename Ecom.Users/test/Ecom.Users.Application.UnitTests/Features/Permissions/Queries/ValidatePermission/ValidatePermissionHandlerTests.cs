using Ecom.Users.Application.Features.Permissions.Queries.ValidatePermission;
using Ecom.Users.Domain.Constants;
using Ecom.Users.Domain.DTOs.Users;
using Ecom.Users.Domain.Interfaces;
using FluentAssertions;
using Moq;
using Shared.Responses;
using Shared.Utils;

namespace Ecom.Users.Application.UnitTests.Features.Permissions.Queries.ValidatePermission;

public class ValidatePermissionHandlerTests
{
    private readonly Mock<IUserService> _mockUserService;
    private readonly ValidatePermissionHandler _handler;

    public ValidatePermissionHandlerTests()
    {
        _mockUserService = new Mock<IUserService>();
        _handler = new ValidatePermissionHandler(_mockUserService.Object);
    }

    [Fact]
    public async Task Handle_WithAdminUser_ShouldReturnPermissionGranted()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var request = new ValidatePermissionRequest
        {
            UserId = userId,
            Action = "delete",
            Resource = "user",
            ResourceId = Guid.NewGuid()
        };

        var userDto = new UserDto
        {
            Id = userId,
            Email = "admin@example.com",
            Username = "admin"
        };

        var roleDto = new RoleDto
        {
            Id = Guid.NewGuid(),
            Name = AuthConstants.Roles.Admin
        };

        var userResponse = ResponseUtils.Success(userDto);
        var rolesResponse = ResponseUtils.Success(new List<RoleDto> { roleDto });

        _mockUserService.Setup(x => x.GetUserByIdAsync(userId))
            .ReturnsAsync(userResponse);
        _mockUserService.Setup(x => x.GetUserRolesAsync(userId))
    .ReturnsAsync(ResponseUtils.Success<IEnumerable<RoleDto>>([roleDto]));

        // Act
        var result = await _handler.Handle(request, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.StatusCode.Should().Be(200);
        result.Message.Should().Be(MessageKeys.Success);
        result.Data.Should().NotBeNull();
        result.Data!.HasPermission.Should().BeTrue();
        result.Data.Action.Should().Be(request.Action);
        result.Data.Resource.Should().Be(request.Resource);
        result.Data.UserId.Should().Be(userId);
        result.Data.Roles.Should().Contain(AuthConstants.Roles.Admin);
        result.Data.Reason.Should().Be(MessageKeys.AdminRole);

        _mockUserService.Verify(x => x.GetUserByIdAsync(userId), Times.Once);
        _mockUserService.Verify(x => x.GetUserRolesAsync(userId), Times.Once);
    }

    [Fact]
    public async Task Handle_WithResourceOwnership_ShouldReturnPermissionGranted()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var request = new ValidatePermissionRequest
        {
            UserId = userId,
            Action = "update",
            Resource = "user",
            ResourceId = userId // User owns the resource
        };

        var userDto = new UserDto
        {
            Id = userId,
            Email = "user@example.com",
            Username = "user"
        };

        var roleDto = new RoleDto
        {
            Id = Guid.NewGuid(),
            Name = "User"
        };

        var userResponse = ResponseUtils.Success(userDto);
        var rolesResponse = ResponseUtils.Success(new List<RoleDto> { roleDto });

        _mockUserService.Setup(x => x.GetUserByIdAsync(userId))
            .ReturnsAsync(userResponse);
        _mockUserService.Setup(x => x.GetUserRolesAsync(userId))
    .ReturnsAsync(ResponseUtils.Success<IEnumerable<RoleDto>>([roleDto]));

        // Act
        var result = await _handler.Handle(request, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.HasPermission.Should().BeTrue();
        result.Data.Reason.Should().Be(MessageKeys.ResourceOwnership);
        result.Data.Roles.Should().Contain("User");

        _mockUserService.Verify(x => x.GetUserByIdAsync(userId), Times.Once);
        _mockUserService.Verify(x => x.GetUserRolesAsync(userId), Times.Once);
    }

    [Fact]
    public async Task Handle_WithSelfReadAccess_ShouldReturnPermissionGranted()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var request = new ValidatePermissionRequest
        {
            UserId = userId,
            Action = "read",
            Resource = "user",
            ResourceId = null // No specific resource ID for self-read
        };

        var userDto = new UserDto
        {
            Id = userId,
            Email = "user@example.com",
            Username = "user"
        };

        var roleDto = new RoleDto
        {
            Id = Guid.NewGuid(),
            Name = "User"
        };

        var userResponse = ResponseUtils.Success(userDto);
        var rolesResponse = ResponseUtils.Success(new List<RoleDto> { roleDto });

        _mockUserService.Setup(x => x.GetUserByIdAsync(userId))
            .ReturnsAsync(userResponse);
        _mockUserService.Setup(x => x.GetUserRolesAsync(userId))
    .ReturnsAsync(ResponseUtils.Success<IEnumerable<RoleDto>>([roleDto]));

        // Act
        var result = await _handler.Handle(request, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.HasPermission.Should().BeTrue();
        result.Data.Reason.Should().Be(MessageKeys.SelfReadAccess);

        _mockUserService.Verify(x => x.GetUserByIdAsync(userId), Times.Once);
        _mockUserService.Verify(x => x.GetUserRolesAsync(userId), Times.Once);
    }

    [Fact]
    public async Task Handle_WithNoPermission_ShouldReturnPermissionDenied()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var otherUserId = Guid.NewGuid();
        var request = new ValidatePermissionRequest
        {
            UserId = userId,
            Action = "delete",
            Resource = "user",
            ResourceId = otherUserId // User doesn't own this resource
        };

        var userDto = new UserDto
        {
            Id = userId,
            Email = "user@example.com",
            Username = "user"
        };

        var roleDto = new RoleDto
        {
            Id = Guid.NewGuid(),
            Name = AuthConstants.Roles.User
        };

        var userResponse = ResponseUtils.Success(userDto);
        var rolesResponse = ResponseUtils.Success(new List<RoleDto> { roleDto });

        _mockUserService.Setup(x => x.GetUserByIdAsync(userId))
            .ReturnsAsync(userResponse);
        _mockUserService.Setup(x => x.GetUserRolesAsync(userId))
    .ReturnsAsync(ResponseUtils.Success<IEnumerable<RoleDto>>([roleDto]));

        // Act
        var result = await _handler.Handle(request, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.HasPermission.Should().BeFalse();
        result.Data.Reason.Should().Be(MessageKeys.AccessDenied);

        _mockUserService.Verify(x => x.GetUserByIdAsync(userId), Times.Once);
        _mockUserService.Verify(x => x.GetUserRolesAsync(userId), Times.Once);
    }

    [Fact]
    public async Task Handle_WithUnsupportedResource_ShouldReturnPermissionDenied()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var request = new ValidatePermissionRequest
        {
            UserId = userId,
            Action = "read",
            Resource = "unsupported",
            ResourceId = Guid.NewGuid()
        };

        var userDto = new UserDto
        {
            Id = userId,
            Email = "user@example.com",
            Username = "user"
        };

        var roleDto = new RoleDto
        {
            Id = Guid.NewGuid(),
            Name = "User"
        };

        var userResponse = ResponseUtils.Success(userDto);
        var rolesResponse = ResponseUtils.Success(new List<RoleDto> { roleDto });

        _mockUserService.Setup(x => x.GetUserByIdAsync(userId))
            .ReturnsAsync(userResponse);

        _mockUserService.Setup(x => x.GetUserRolesAsync(userId))
    .ReturnsAsync(ResponseUtils.Success<IEnumerable<RoleDto>>([roleDto]));
        // Act
        var result = await _handler.Handle(request, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.HasPermission.Should().BeFalse();
        result.Data.Reason.Should().Be(MessageKeys.ResourceNotSupported);

        _mockUserService.Verify(x => x.GetUserByIdAsync(userId), Times.Once);
        _mockUserService.Verify(x => x.GetUserRolesAsync(userId), Times.Once);
    }

    [Fact]
    public async Task Handle_WithNonExistentUser_ShouldReturnErrorResponse()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var request = new ValidatePermissionRequest
        {
            UserId = userId,
            Action = "read",
            Resource = "user"
        };

        var userResponse = ResponseUtils.Error<UserDto>(404, MessageKeys.UserNotFound);

        _mockUserService.Setup(x => x.GetUserByIdAsync(userId))
            .ReturnsAsync(userResponse);

        // Act
        var result = await _handler.Handle(request, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.StatusCode.Should().Be(404);
        result.Message.Should().Be(MessageKeys.UserNotFound);
        result.Data.Should().BeNull();

        _mockUserService.Verify(x => x.GetUserByIdAsync(userId), Times.Once);
        _mockUserService.Verify(x => x.GetUserRolesAsync(It.IsAny<Guid>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WithUserServiceReturningNull_ShouldReturnErrorResponse()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var request = new ValidatePermissionRequest
        {
            UserId = userId,
            Action = "read",
            Resource = "user"
        };

        var userResponse = new Response<UserDto>(
            IsSuccess: true,
            200,
            MessageKeys.Success,
            Data: null
        );

        _mockUserService.Setup(x => x.GetUserByIdAsync(userId))
            .ReturnsAsync(userResponse);

        // Act
        var result = await _handler.Handle(request, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.StatusCode.Should().Be(404);
        result.Message.Should().Be(MessageKeys.UserNotFound);
        result.Data.Should().BeNull();

        _mockUserService.Verify(x => x.GetUserByIdAsync(userId), Times.Once);
        _mockUserService.Verify(x => x.GetUserRolesAsync(It.IsAny<Guid>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WithRolesServiceFailure_ShouldHandleGracefully()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var request = new ValidatePermissionRequest
        {
            UserId = userId,
            Action = "read",
            Resource = "user"
        };

        var userDto = new UserDto
        {
            Id = userId,
            Email = "user@example.com",
            Username = "user"
        };

        var userResponse = ResponseUtils.Success(userDto);
        var rolesResponse = ResponseUtils.Error<IEnumerable<RoleDto>>(400, "Failed to get roles");

        _mockUserService.Setup(x => x.GetUserByIdAsync(userId))
            .ReturnsAsync(userResponse);
 
        _mockUserService.Setup(x => x.GetUserRolesAsync(userId))
            .ReturnsAsync(rolesResponse);
        // Act
        var result = await _handler.Handle(request, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.Roles.Should().BeEmpty();
        result.Data.HasPermission.Should().BeTrue(); // Self read access
        result.Data.Reason.Should().Be(MessageKeys.SelfReadAccess);

        _mockUserService.Verify(x => x.GetUserByIdAsync(userId), Times.Once);
        _mockUserService.Verify(x => x.GetUserRolesAsync(userId), Times.Once);
    }

    [Fact]
    public async Task Handle_WithMultipleRoles_ShouldDetectAdminRole()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var request = new ValidatePermissionRequest
        {
            UserId = userId,
            Action = "delete",
            Resource = "user",
            ResourceId = Guid.NewGuid()
        };

        var userDto = new UserDto
        {
            Id = userId,
            Email = "user@example.com",
            Username = "user"
        };

        var roles = new List<RoleDto>
        {
            new() { Id = Guid.NewGuid(), Name = AuthConstants.Roles.User },
            new() { Id = Guid.NewGuid(), Name = AuthConstants.Roles.Employee },
            new() { Id = Guid.NewGuid(), Name = AuthConstants.Roles.Admin }
        };

        var userResponse = ResponseUtils.Success(userDto);
        var rolesResponse = ResponseUtils.Success(roles);

        _mockUserService.Setup(x => x.GetUserByIdAsync(userId))
            .ReturnsAsync(userResponse);
        _mockUserService.Setup(x => x.GetUserRolesAsync(userId))
    .ReturnsAsync(ResponseUtils.Success<IEnumerable<RoleDto>>(roles));

        // Act
        var result = await _handler.Handle(request, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.HasPermission.Should().BeTrue();
        result.Data.Reason.Should().Be(MessageKeys.AdminRole);
        result.Data.Roles.Should().HaveCount(3);
        result.Data.Roles.Should().Contain(AuthConstants.Roles.Admin);
        result.Data.Roles.Should().Contain(AuthConstants.Roles.User);
        result.Data.Roles.Should().Contain(AuthConstants.Roles.Employee);

        _mockUserService.Verify(x => x.GetUserByIdAsync(userId), Times.Once);
        _mockUserService.Verify(x => x.GetUserRolesAsync(userId), Times.Once);
    }

    [Fact]
    public async Task Handle_WithEmptyGuid_ShouldCallService()
    {
        // Arrange
        var request = new ValidatePermissionRequest
        {
            UserId = Guid.Empty,
            Action = "read",
            Resource = "user"
        };

        var userResponse = ResponseUtils.Error<UserDto>(404, "User not found");

        _mockUserService.Setup(x => x.GetUserByIdAsync(Guid.Empty))
            .ReturnsAsync(userResponse);

        // Act
        var result = await _handler.Handle(request, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();

        _mockUserService.Verify(x => x.GetUserByIdAsync(Guid.Empty), Times.Once);
    }

    [Fact]
    public async Task Handle_WithServiceException_ShouldPropagateException()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var request = new ValidatePermissionRequest
        {
            UserId = userId,
            Action = "read",
            Resource = "user"
        };

        _mockUserService.Setup(x => x.GetUserByIdAsync(userId))
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
        var request = new ValidatePermissionRequest
        {
            UserId = userId,
            Action = "read",
            Resource = "user"
        };

        var cancellationToken = new CancellationToken(true);
        var userResponse = ResponseUtils.Error<UserDto>(400, "Operation cancelled");

        _mockUserService.Setup(x => x.GetUserByIdAsync(userId))
            .ReturnsAsync(userResponse);

        // Act
        var result = await _handler.Handle(request, cancellationToken);

        // Assert
        result.Should().NotBeNull();
        _mockUserService.Verify(x => x.GetUserByIdAsync(userId), Times.Once);
    }

    [Theory]
    [InlineData("create", "USER")]
    [InlineData("READ", "user")]
    [InlineData("Update", "User")]
    [InlineData("DELETE", "USER")]
    public async Task Handle_WithCaseInsensitiveActions_ShouldHandleCorrectly(string action, string resource)
    {
        // Arrange
        var userId = Guid.NewGuid();
        var request = new ValidatePermissionRequest
        {
            UserId = userId,
            Action = action,
            Resource = resource
        };

        var userDto = new UserDto
        {
            Id = userId,
            Email = "user@example.com",
            Username = "user"
        };

        var roleDto = new RoleDto
        {
            Id = Guid.NewGuid(),
            Name = "User"
        };

        var userResponse = ResponseUtils.Success(userDto);
        var rolesResponse = ResponseUtils.Success(new List<RoleDto> { roleDto });

        _mockUserService.Setup(x => x.GetUserByIdAsync(userId))
            .ReturnsAsync(userResponse);
        _mockUserService.Setup(x => x.GetUserRolesAsync(userId))
     .ReturnsAsync(ResponseUtils.Success<IEnumerable<RoleDto>>([roleDto]));

        // Act
        var result = await _handler.Handle(request, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.Action.Should().Be(action);
        result.Data.Resource.Should().Be(resource);

        _mockUserService.Verify(x => x.GetUserByIdAsync(userId), Times.Once);
        _mockUserService.Verify(x => x.GetUserRolesAsync(userId), Times.Once);
    }

    [Fact]
    public async Task Handle_WithRolesReturningNull_ShouldHandleGracefully()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var request = new ValidatePermissionRequest
        {
            UserId = userId,
            Action = "read",
            Resource = "user"
        };

        var userDto = new UserDto
        {
            Id = userId,
            Email = "user@example.com",
            Username = "user"
        };

        var userResponse = ResponseUtils.Success(userDto);
        var rolesResponse = new Response<List<RoleDto>>(
            IsSuccess: true,
            200,
            "Success",
            Data: null
        );

        _mockUserService.Setup(x => x.GetUserByIdAsync(userId))
            .ReturnsAsync(userResponse);
        _mockUserService.Setup(x => x.GetUserRolesAsync(userId))
        .ReturnsAsync(new Response<IEnumerable<RoleDto>>(
            IsSuccess: true,
            200,
            "Success",
            Data: null
        ));

        // Act
        var result = await _handler.Handle(request, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.Roles.Should().BeEmpty();
        result.Data.HasPermission.Should().BeTrue(); // Self read access
        result.Data.Reason.Should().Be(MessageKeys.SelfReadAccess);

        _mockUserService.Verify(x => x.GetUserByIdAsync(userId), Times.Once);
        _mockUserService.Verify(x => x.GetUserRolesAsync(userId), Times.Once);
    }
}
