using Domain.Builders;
using Ecom.Users.Application.Services;
using Ecom.Users.Domain.Constants;
using Ecom.Users.Domain.DTOs;
using Ecom.Users.Domain.Entities;
using Ecom.Users.Domain.Interfaces;
using EfCore.IRepositories;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Shared.Utils;
using System.Linq.Expressions;

namespace Ecom.Users.Application.UnitTests.Services;

public class UserServiceTests
{
    private readonly Mock<ICommandRepository> _mockCommandRepository;
    private readonly Mock<IQueryRepository<User, Guid>> _mockUserRepository;
    private readonly Mock<IQueryRepository<Role, Guid>> _mockRoleRepository;
    private readonly Mock<IQueryRepository<UserRole, Guid>> _mockUserRoleRepository;
    private readonly Mock<IQueryRepository<RolePermission, Guid>> _mockRolePermissionRepository;
    private readonly Mock<IPasswordHasher> _mockPasswordHasher;
    private readonly Mock<ILogger<UserService>> _mockLogger;
    private readonly UserService _userService;

    public UserServiceTests()
    {
        _mockCommandRepository = new Mock<ICommandRepository>();
        _mockUserRepository = new Mock<IQueryRepository<User, Guid>>();
        _mockRoleRepository = new Mock<IQueryRepository<Role, Guid>>();
        _mockUserRoleRepository = new Mock<IQueryRepository<UserRole, Guid>>();
        _mockRolePermissionRepository = new Mock<IQueryRepository<RolePermission, Guid>>();
        _mockPasswordHasher = new Mock<IPasswordHasher>();
        _mockLogger = new Mock<ILogger<UserService>>();

        _userService = new UserService(
            _mockCommandRepository.Object,
            _mockUserRepository.Object,
            _mockRoleRepository.Object,
            _mockUserRoleRepository.Object,
            _mockRolePermissionRepository.Object,
            _mockPasswordHasher.Object,
            _mockLogger.Object
        );
    }

    [Fact]
    public async Task GetUserByIdAsync_WithValidId_ShouldReturnSuccessResponse()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = new User
        {
            Id = userId,
            Email = "test@example.com",
            UserName = "testuser",
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

        _mockUserRepository.Setup(x => x.FirstOrDefaultAsync(
            It.IsAny<Expression<Func<User, bool>>>(), // Correct usage of Expression<Func<User, bool>>
            It.IsAny<Action<IFluentBuilder<User>>?>(),
            It.IsAny<bool>(),
            It.IsAny<CancellationToken>()
        )).ReturnsAsync(user);

        // Act
        var result = await _userService.GetUserByIdAsync(userId);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.Id.Should().Be(userId);
        result.Data.Email.Should().Be(user.Email);
        result.Data.Username.Should().Be(user.UserName);
        result.Data.FirstName.Should().Be(user.FirstName);
        result.Data.LastName.Should().Be(user.LastName);
        result.Data.PhoneNumber.Should().Be(user.PhoneNumber);
        result.Data.EmailConfirmed.Should().Be(user.EmailConfirmed);
        result.Data.PhoneNumberConfirmed.Should().Be(user.PhoneNumberConfirmed);
        result.Data.TwoFactorEnabled.Should().Be(user.TwoFactorEnabled);
        result.Data.Provider.Should().Be(user.Provider);
    }

    [Fact]
    public async Task GetUserByIdAsync_WithInvalidId_ShouldReturnErrorResponse()
    {
        // Arrange
        var userId = Guid.NewGuid();

        _mockUserRepository.Setup(x => x.FirstOrDefaultAsync(
        It.IsAny<Expression<Func<User, bool>>>(),
        It.IsAny<Action<IFluentBuilder<User>>?>(),
        It.IsAny<bool>(),
        It.IsAny<CancellationToken>()
    )).ReturnsAsync((User?)null);

        // Act
        var result = await _userService.GetUserByIdAsync(userId);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.StatusCode.Should().Be(204);
        result.Message.Should().Be(MessageKeys.UserNotFound);
        result.Data.Should().BeNull();
    }

    [Fact]
    public async Task GetUserByEmailAsync_WithValidEmail_ShouldReturnSuccessResponse()
    {
        // Arrange
        var email = "test@example.com";
        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = email,
            UserName = "testuser",
            FirstName = "John",
            LastName = "Doe",
            EmailConfirmed = true,
            Provider = AuthConstants.Providers.Local,
        };

        _mockUserRepository.Setup(x => x.FirstOrDefaultAsync(
            It.IsAny<Expression<Func<User, bool>>>(),
            It.IsAny<Action<IFluentBuilder<User>>?>(),
            It.IsAny<bool>(),
            It.IsAny<CancellationToken>()
        )).ReturnsAsync(user);
        // Act
        var result = await _userService.GetUserByEmailAsync(email);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.Email.Should().Be(email);
        result.Data.Username.Should().Be(user.UserName);
    }

    [Fact]
    public async Task GetUserByEmailAsync_WithInvalidEmail_ShouldReturnErrorResponse()
    {
        // Arrange
        var email = "nonexistent@example.com";

        _mockUserRepository.Setup(x => x.FirstOrDefaultAsync(
    It.IsAny<Expression<Func<User, bool>>>(),
    It.IsAny<Action<IFluentBuilder<User>>?>(),
    It.IsAny<bool>(),
    It.IsAny<CancellationToken>()
)).ReturnsAsync((User?)null);

        // Act
        var result = await _userService.GetUserByEmailAsync(email);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.StatusCode.Should().Be(204);
        result.Message.Should().Be(MessageKeys.UserNotFound);
        result.Data.Should().BeNull();
    }

    [Fact]
    public async Task UpdateUserAsync_WithValidData_ShouldReturnSuccessResponse()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var updateDto = new UpdateUserDto
        {
            FirstName = "UpdatedFirstName",
            LastName = "UpdatedLastName",
            UserName = "updatedusername",
            PhoneNumber = "9876543210"
        };

        var existingUser = new User
        {
            Id = userId,
            Email = "test@example.com",
            UserName = "oldusername",
            FirstName = "OldFirstName",
            LastName = "OldLastName",
            PhoneNumber = "1234567890",
            EmailConfirmed = true,
            CreatedAt = DateTimeOffset.UtcNow.AddDays(-10),
            UpdatedAt = DateTimeOffset.UtcNow.AddDays(-5),
            Provider = AuthConstants.Providers.Local,
        };

        _mockUserRepository.Setup(x => x.FirstOrDefaultAsync(
            It.IsAny<Expression<Func<User, bool>>>(),
            It.IsAny<Action<IFluentBuilder<User>>?>(),
            It.IsAny<bool>(),
            It.IsAny<CancellationToken>()
        )).ReturnsAsync(existingUser);

        _mockUserRepository.Setup(x => x.AnyAsync(
            It.IsAny<Expression<Func<User, bool>>>(),
            It.IsAny<CancellationToken>()
        )).ReturnsAsync(false);

        _mockCommandRepository.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        var result = await _userService.UpdateUserAsync(userId, updateDto);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.FirstName.Should().Be(updateDto.FirstName);
        result.Data.LastName.Should().Be(updateDto.LastName);
        result.Data.Username.Should().Be(updateDto.UserName);
        result.Data.PhoneNumber.Should().Be(updateDto.PhoneNumber);

        // Verify that the user entity was updated
        existingUser.FirstName.Should().Be(updateDto.FirstName);
        existingUser.LastName.Should().Be(updateDto.LastName);
        existingUser.UserName.Should().Be(updateDto.UserName);
        existingUser.PhoneNumber.Should().Be(updateDto.PhoneNumber);
        existingUser.UpdatedBy.Should().Be(userId);
        existingUser.UpdatedAt.Should().BeCloseTo(DateTimeOffset.UtcNow, TimeSpan.FromSeconds(5));

        _mockCommandRepository.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpdateUserAsync_WithNonExistentUser_ShouldReturnErrorResponse()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var updateDto = new UpdateUserDto
        {
            FirstName = "UpdatedFirstName",
            LastName = "UpdatedLastName"
        };

        _mockUserRepository.Setup(x => x.FirstOrDefaultAsync(
            It.IsAny<Expression<Func<User, bool>>>(),
            It.IsAny<Action<IFluentBuilder<User>>?>(),
            It.IsAny<bool>(),
            It.IsAny<CancellationToken>()
        )).ReturnsAsync((User?)null);

        // Act
        var result = await _userService.UpdateUserAsync(userId, updateDto);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.StatusCode.Should().Be(204);
        result.Message.Should().Be(MessageKeys.UserNotFound);
        result.Data.Should().BeNull();

        _mockCommandRepository.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task AssignRoleToUserAsync_WithValidData_ShouldReturnSuccessResponse()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var roleId = Guid.NewGuid();

        var user = new User { Id = userId, Email = "test@example.com" };
        var role = new Role { Id = roleId, Name = "TestRole" };

        _mockUserRepository.Setup(x => x.FirstOrDefaultAsync(
     It.IsAny<Expression<Func<User, bool>>>(),
     It.IsAny<Action<IFluentBuilder<User>>?>(),
     It.IsAny<bool>(),
     It.IsAny<CancellationToken>()))
     .ReturnsAsync(user);

        _mockRoleRepository.Setup(x => x.FirstOrDefaultAsync(
            It.IsAny<Expression<Func<Role, bool>>>(),
            It.IsAny<Action<IFluentBuilder<Role>>?>(),
            It.IsAny<bool>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(role);

        _mockUserRoleRepository.Setup(x => x.FirstOrDefaultAsync(
            It.IsAny<Expression<Func<UserRole, bool>>>(),
            It.IsAny<Action<IFluentBuilder<UserRole>>?>(),
            It.IsAny<bool>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync((UserRole?)null);

        _mockCommandRepository.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        var result = await _userService.AssignRoleToUserAsync(userId, roleId);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().BeTrue();

        _mockCommandRepository.Verify(x => x.AddAsync(It.IsAny<UserRole>(), It.IsAny<CancellationToken>()), Times.Once);
        _mockCommandRepository.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task AssignRoleToUserAsync_WithNonExistentUser_ShouldReturnErrorResponse()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var roleId = Guid.NewGuid();

        _mockUserRepository.Setup(x => x.FirstOrDefaultAsync(
      It.IsAny<Expression<Func<User, bool>>>(),
      It.IsAny<Action<IFluentBuilder<User>>?>(),
      It.IsAny<bool>(),
      It.IsAny<CancellationToken>()
  )).ReturnsAsync((User?)null);

        // Act
        var result = await _userService.AssignRoleToUserAsync(userId, roleId);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.StatusCode.Should().Be(204);
        result.Message.Should().Be(MessageKeys.UserNotFound);
        result.Data.Should().BeFalse();

        _mockCommandRepository.Verify(x => x.AddAsync(It.IsAny<UserRole>(), It.IsAny<CancellationToken>()), Times.Never);
        _mockCommandRepository.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task RemoveRoleFromUserAsync_WithValidData_ShouldReturnSuccessResponse()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var roleId = Guid.NewGuid();

        var userRole = new UserRole
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            RoleId = roleId
        };

        _mockUserRoleRepository.Setup(x => x.FirstOrDefaultAsync(
        It.IsAny<Expression<Func<UserRole, bool>>>(),
        It.IsAny<Action<IFluentBuilder<UserRole>>?>(),
        It.IsAny<bool>(),
        It.IsAny<CancellationToken>()))
    .ReturnsAsync(userRole);

        _mockCommandRepository.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        var result = await _userService.RemoveRoleFromUserAsync(userId, roleId);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().BeTrue();

        _mockCommandRepository.Verify(x => x.DeleteAsync(It.IsAny<UserRole>(), It.IsAny<CancellationToken>()), Times.Once);
        _mockCommandRepository.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task HasPermissionAsync_WithValidPermission_ShouldReturnTrue()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var module = "TestModule";
        var action = "Read";
        var resource = "TestResource";

        var user = new User
        {
            Id = userId,
            Email = "test@example.com",
            UserName = "testuser",
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

        _mockUserRepository.Setup(x => x.FirstOrDefaultAsync(
            It.IsAny<Expression<Func<User, bool>>>(),
            It.IsAny<Action<IFluentBuilder<User>>?>(),
            It.IsAny<bool>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        var roleId = Guid.NewGuid();
        var userRoleIds = new List<UserRole>
        {
            new() { RoleId = roleId, UserId = userId, Role = new Role { Id = roleId, Name = "TestRole" , RolePermissions = new List<RolePermission>
            {
                new() {
                    Permission = new Permission
                    {
                        Module = module,
                        Action = action,
                        Resource = resource
                    }
                }
            } } }
        };

        _mockUserRoleRepository.Setup(x => x.GetAllAsync(
            It.IsAny<Expression<Func<UserRole, bool>>>(),
            It.IsAny<Action<IFluentBuilder<UserRole>>>(),
            It.IsAny<Pagination>(),
            It.IsAny<bool>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync((userRoleIds, new Pagination()));

        _mockRolePermissionRepository.Setup(x => x.AnyAsync(
            It.IsAny<Expression<Func<RolePermission, bool>>>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await _userService.HasPermissionAsync(userId, module, action, resource);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task HasPermissionAsync_WithInvalidPermission_ShouldReturnFalse()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var module = "TestModule";
        var action = "Delete";
        var resource = "TestResource";

        var userRoleIds = new List<UserRole>
        {
            new() { RoleId = Guid.NewGuid() }
        };

        _mockUserRoleRepository.Setup(x => x.GetAllAsync(
            It.IsAny<Expression<Func<UserRole, bool>>>(),
            It.IsAny<Action<IFluentBuilder<UserRole>>>(),
            It.IsAny<Pagination>(),
            It.IsAny<bool>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync((userRoleIds, new Pagination()));

        _mockRolePermissionRepository.Setup(x => x.AnyAsync(
            It.IsAny<Expression<Func<RolePermission, bool>>>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act
        var result = await _userService.HasPermissionAsync(userId, module, action, resource);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task DeleteUserAsync_WithValidId_ShouldReturnSuccessResponse()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = new User
        {
            Id = userId,
            Email = "test@example.com",
            IsDeleted = false
        };

        _mockUserRepository.Setup(x => x.FirstOrDefaultAsync(It.IsAny<Expression<Func<User, bool>>>(), It.IsAny<Action<IFluentBuilder<User>>?>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _mockCommandRepository.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        var result = await _userService.DeleteUserAsync(userId);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().BeTrue();

        user.IsDeleted.Should().BeTrue();
        user.DeletedAt.Should().BeCloseTo(DateTimeOffset.UtcNow, TimeSpan.FromSeconds(5));
        user.DeletedBy.Should().Be(userId);

        _mockCommandRepository.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public async Task GetUserByEmailAsyncWithInvalidEmailShouldReturnErrorResponse(string? email)
    {
        // Act
        var result = await _userService.GetUserByEmailAsync(email!);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.StatusCode.Should().Be(400);
        result.Message.Should().Be(MessageKeys.EmailRequired);
        result.Data.Should().BeNull();

        _mockUserRepository.Verify(x => x.FirstOrDefaultAsync(It.IsAny<Expression<Func<User, bool>>>(), It.IsAny<Action<IFluentBuilder<User>>?>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}
