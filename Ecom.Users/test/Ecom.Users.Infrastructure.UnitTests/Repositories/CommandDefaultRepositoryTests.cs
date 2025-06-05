using Ecom.Users.Domain.Entities;
using Ecom.Users.Infrastructure.Persistence.EfCore;
using Ecom.Users.Infrastructure.Repositories;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;

namespace Ecom.Users.Infrastructure.UnitTests.Repositories;

public class CommandDefaultRepositoryTests : IDisposable
{
    private readonly ApplicationDbContext _dbContext;
    private readonly Mock<ILogger<CommandDefaultRepository>> _mockLogger;
    private readonly CommandDefaultRepository _repository;

    public CommandDefaultRepositoryTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _dbContext = new ApplicationDbContext(options);
        _mockLogger = new Mock<ILogger<CommandDefaultRepository>>();
        _repository = new CommandDefaultRepository(_dbContext, _mockLogger.Object);
    }

    [Fact]
    public async Task AddAsync_ShouldAddEntityToDatabase()
    {
        // Arrange
        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = "test@example.com",
            UserName = "testuser",
            PasswordHash = "hashedPassword",
            EmailConfirmed = true,
            CreatedAt = DateTimeOffset.UtcNow,
            CreatedBy = Guid.NewGuid()
        };

        // Act
        await _repository.AddAsync(user);
        await _repository.SaveChangesAsync();

        // Assert
        var savedUser = await _dbContext.Set<User>().FindAsync(user.Id);
        savedUser.Should().NotBeNull();
        savedUser!.Email.Should().Be(user.Email);
        savedUser.UserName.Should().Be(user.UserName);
    }

    [Fact]
    public async Task UpdateAsync_ShouldUpdateEntityInDatabase()
    {
        // Arrange
        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = "test@example.com",
            UserName = "testuser",
            PasswordHash = "hashedPassword",
            EmailConfirmed = true,
            CreatedAt = DateTimeOffset.UtcNow,
            CreatedBy = Guid.NewGuid()
        };

        await _dbContext.Set<User>().AddAsync(user);
        await _dbContext.SaveChangesAsync();

        // Act
        user.UserName = "updateduser";
        user.UpdatedAt = DateTimeOffset.UtcNow;
        await _repository.UpdateAsync(user);
        await _repository.SaveChangesAsync();

        // Assert
        var updatedUser = await _dbContext.Set<User>().FindAsync(user.Id);
        updatedUser.Should().NotBeNull();
        updatedUser!.UserName.Should().Be("updateduser");
        updatedUser.UpdatedAt.Should().NotBeNull();
    }

    [Fact]
    public async Task DeleteAsync_ShouldRemoveEntityFromDatabase()
    {
        // Arrange
        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = "test@example.com",
            UserName = "testuser",
            PasswordHash = "hashedPassword",
            EmailConfirmed = true,
            CreatedAt = DateTimeOffset.UtcNow,
            CreatedBy = Guid.NewGuid()
        };

        await _dbContext.Set<User>().AddAsync(user);
        await _dbContext.SaveChangesAsync();

        // Act
        await _repository.DeleteAsync(user);
        await _repository.SaveChangesAsync();

        // Assert
        var deletedUser = await _dbContext.Set<User>().FindAsync(user.Id);
        deletedUser.Should().BeNull();
    }

    [Fact]
    public async Task SaveChangesAsync_ShouldReturnNumberOfAffectedEntries()
    {
        // Arrange
        var user1 = new User
        {
            Id = Guid.NewGuid(),
            Email = "test1@example.com",
            UserName = "testuser1",
            PasswordHash = "hashedPassword",
            EmailConfirmed = true,
            CreatedAt = DateTimeOffset.UtcNow,
            CreatedBy = Guid.NewGuid()
        };

        var user2 = new User
        {
            Id = Guid.NewGuid(),
            Email = "test2@example.com",
            UserName = "testuser2",
            PasswordHash = "hashedPassword",
            EmailConfirmed = true,
            CreatedAt = DateTimeOffset.UtcNow,
            CreatedBy = Guid.NewGuid()
        };

        await _repository.AddAsync(user1);
        await _repository.AddAsync(user2);

        // Act
        var result = await _repository.SaveChangesAsync();

        // Assert
        result.Should().Be(2);
    }

    [Fact]
    public async Task AddRangeAsync_ShouldAddMultipleEntities()
    {
        // Arrange
        var users = new List<User>
        {
            new()
            {
                Id = Guid.NewGuid(),
                Email = "test1@example.com",
                UserName = "testuser1",
                PasswordHash = "hashedPassword",
                EmailConfirmed = true,
                CreatedAt = DateTimeOffset.UtcNow,
                CreatedBy = Guid.NewGuid()
            },
            new()
            {
                Id = Guid.NewGuid(),
                Email = "test2@example.com",
                UserName = "testuser2",
                PasswordHash = "hashedPassword",
                EmailConfirmed = true,
                CreatedAt = DateTimeOffset.UtcNow,
                CreatedBy = Guid.NewGuid()
            }
        };

        // Act
        await _repository.AddRangeAsync(users);
        await _repository.SaveChangesAsync();

        // Assert
        var savedUsers = await _dbContext.Set<User>().ToListAsync();
        savedUsers.Should().HaveCount(2);
        savedUsers.Should().Contain(u => u.Email == "test1@example.com");
        savedUsers.Should().Contain(u => u.Email == "test2@example.com");
    }

    public void Dispose()
    {
        _dbContext.Dispose();
    }
}
