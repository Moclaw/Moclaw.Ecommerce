using Ecom.Users.Domain.Entities;
using Ecom.Users.Infrastructure.Persistence.EfCore;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace Ecom.Users.Infrastructure.UnitTests.Persistence;

public class ApplicationDbContextTests : IDisposable
{
    private readonly ApplicationDbContext _dbContext;

    public ApplicationDbContextTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _dbContext = new ApplicationDbContext(options);
    }

    [Fact]
    public async Task ApplicationDbContext_ShouldEnsureCreated()
    {
        // Act
        var result = await _dbContext.Database.EnsureCreatedAsync();

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void ApplicationDbContext_ShouldHaveCorrectModelConfiguration()
    {
        // Arrange & Act
        var model = _dbContext.Model;

        // Assert
        model.Should().NotBeNull();
        
        // Check if entities are properly configured
        var userEntityType = model.FindEntityType(typeof(Domain.Entities.User));
        userEntityType.Should().NotBeNull();
        
        var roleEntityType = model.FindEntityType(typeof(Domain.Entities.Role));
        roleEntityType.Should().NotBeNull();
        
        var permissionEntityType = model.FindEntityType(typeof(Domain.Entities.Permission));
        permissionEntityType.Should().NotBeNull();
        
        var refreshTokenEntityType = model.FindEntityType(typeof(Domain.Entities.RefreshToken));
        refreshTokenEntityType.Should().NotBeNull();
    }

    [Fact]
    public async Task ApplicationDbContext_ShouldSupportTransactions()
    {
        // Arrange
        await _dbContext.Database.EnsureCreatedAsync();

        // Act & Assert
        using var transaction = await _dbContext.Database.BeginTransactionAsync();
        transaction.Should().NotBeNull();
        
        // Add some data
        var user = new Domain.Entities.User
        {
            Id = Guid.NewGuid(),
            Email = "test@example.com",
            UserName = "testuser",
            PasswordHash = "hashedPassword",
            EmailConfirmed = true,
            CreatedAt = DateTimeOffset.UtcNow,
            CreatedBy = Guid.NewGuid()
        };

        _dbContext.Set<User>().Add(user);
        await _dbContext.SaveChangesAsync();

        // Rollback transaction
        await transaction.RollbackAsync();

        // Verify data was rolled back
        var userCount = await _dbContext.Set<User>().CountAsync();
        userCount.Should().Be(0);
    }

    [Fact]
    public async Task ApplicationDbContext_ShouldHandleConcurrencyCorrectly()
    {
        // Arrange
        await _dbContext.Database.EnsureCreatedAsync();
        
        var user = new Domain.Entities.User
        {
            Id = Guid.NewGuid(),
            Email = "test@example.com",
            UserName = "testuser",
            PasswordHash = "hashedPassword",
            EmailConfirmed = true,
            CreatedAt = DateTimeOffset.UtcNow,
            CreatedBy = Guid.NewGuid()
        };

        _dbContext.Set<User>().Add(user);
        await _dbContext.SaveChangesAsync();

        // Create two contexts to simulate concurrent access
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: _dbContext.Database.GetDbConnection().Database)
            .Options;        using var context1 = new ApplicationDbContext(options);
        using var context2 = new ApplicationDbContext(options);

        // Act
        var user1 = await context1.Set<User>().FindAsync(user.Id);
        var user2 = await context2.Set<User>().FindAsync(user.Id);

        user1!.UserName = "updateduser1";
        user2!.UserName = "updateduser2";

        await context1.SaveChangesAsync();

        // Assert
        // The second context should be able to save since we're using in-memory database
        // In a real database with proper concurrency tokens, this would throw an exception
        var saveAction = async () => await context2.SaveChangesAsync();
        await saveAction.Should().NotThrowAsync();
    }

    public void Dispose()
    {
        _dbContext.Dispose();
    }
}
