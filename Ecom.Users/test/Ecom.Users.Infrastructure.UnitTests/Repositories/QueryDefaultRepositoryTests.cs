using Ecom.Users.Domain.Entities;
using Ecom.Users.Infrastructure.Persistence.EfCore;
using Ecom.Users.Infrastructure.Repositories;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Shared.Utils;

namespace Ecom.Users.Infrastructure.UnitTests.Repositories;

public class QueryDefaultRepositoryTests : IDisposable
{
    private readonly ApplicationDbContext _dbContext;
    private readonly QueryDefaultRepository<User, Guid> _repository;

    public QueryDefaultRepositoryTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _dbContext = new ApplicationDbContext(options);
        _repository = new QueryDefaultRepository<User, Guid>(_dbContext);

        SeedData();
    }

    private void SeedData()
    {
        var users = new List<User>
        {
            new()
            {
                Id = Guid.NewGuid(),
                Email = "user1@example.com",
                UserName = "user1",
                FirstName = "John",
                LastName = "Doe",
                PasswordHash = "hashedPassword",
                EmailConfirmed = true,
                CreatedAt = DateTimeOffset.UtcNow,
                CreatedBy = Guid.NewGuid(),
                IsDeleted = false
            },
            new()
            {
                Id = Guid.NewGuid(),
                Email = "user2@example.com",
                UserName = "user2",
                FirstName = "Jane",
                LastName = "Smith",
                PasswordHash = "hashedPassword",
                EmailConfirmed = false,
                CreatedAt = DateTimeOffset.UtcNow,
                CreatedBy = Guid.NewGuid(),
                IsDeleted = false
            },
            new()
            {
                Id = Guid.NewGuid(),
                Email = "user3@example.com",
                UserName = "user3",
                FirstName = "Bob",
                LastName = "Johnson",
                PasswordHash = "hashedPassword",
                EmailConfirmed = true,
                CreatedAt = DateTimeOffset.UtcNow,
                CreatedBy = Guid.NewGuid(),
                IsDeleted = false
            }
        };

        _dbContext.Set<User>().AddRange(users);
        _dbContext.SaveChanges();
    }

    [Fact]
    public async Task GetAllAsync_ShouldReturnAllEntities()
    {
        // Act
        var result = await _repository.GetAllAsync();

        // Assert
        result.Should().NotBeNull();
        result.Entities.Should().HaveCount(3);
    }

    [Fact]
    public async Task GetAllAsync_WithFilter_ShouldReturnFilteredEntities()
    {
        // Act
        var result = await _repository.GetAllAsync(u => u.EmailConfirmed);

        // Assert
        result.Should().NotBeNull();
        result.Entities.Should().HaveCount(2);
        result.Entities.Should().OnlyContain(u => u.EmailConfirmed);
    }

    [Fact]
    public async Task GetAllAsync_WithPagination_ShouldReturnPaginatedResults()
    { // Arrange
        var pagination = new Pagination(0, 1, 2);

        // Act
        var result = await _repository.GetAllAsync(paging: pagination);

        // Assert
        result.Should().NotBeNull();
        result.Entities.Should().HaveCount(2);
        result.Pagination.Should().NotBeNull();
        result.Pagination!.PageIndex.Should().Be(1);
        result.Pagination.PageSize.Should().Be(2);
    }

    [Fact]
    public async Task GetByIdAsync_WithValidId_ShouldReturnEntity()
    {
        // Arrange
        var users = await _dbContext.Set<User>().ToListAsync();
        var firstUser = users.First();

        // Act
        var result = await _repository.GetByIdAsync(firstUser.Id);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(firstUser.Id);
        result.Email.Should().Be(firstUser.Email);
    }

    [Fact]
    public async Task GetByIdAsync_WithInvalidId_ShouldReturnNull()
    {
        // Arrange
        var invalidId = Guid.NewGuid();

        // Act
        var result = await _repository.GetByIdAsync(invalidId);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task FirstOrDefaultAsync_WithValidFilter_ShouldReturnEntity()
    {
        // Act
        var result = await _repository.FirstOrDefaultAsync(u => u.Email == "user1@example.com");

        // Assert
        result.Should().NotBeNull();
        result!.Email.Should().Be("user1@example.com");
        result.UserName.Should().Be("user1");
    }

    [Fact]
    public async Task FirstOrDefaultAsync_WithInvalidFilter_ShouldReturnNull()
    {
        // Act
        var result = await _repository.FirstOrDefaultAsync(
            u => u.Email == "nonexistent@example.com"
        );

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task AnyAsync_WithValidFilter_ShouldReturnTrue()
    {
        // Act
        var result = await _repository.AnyAsync(u => u.EmailConfirmed);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task AnyAsync_WithInvalidFilter_ShouldReturnFalse()
    {
        // Act
        var result = await _repository.AnyAsync(u => u.Email == "nonexistent@example.com");

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task CountAsync_WithFilter_ShouldReturnFilteredCount()
    {
        // Act
        var result = await _repository.CountAsync(u => u.EmailConfirmed);

        // Assert
        result.Should().Be(2);
    }

    [Fact]
    public async Task GetAllAsync_WithProjection_ShouldReturnProjectedResults()
    {
        // Act
        var result = await _repository.GetAllAsync(
            projector: q =>
                q.Select(
                    u =>
                        new
                        {
                            u.Id,
                            u.Email,
                            u.UserName
                        }
                )
        );

        // Assert
        result.Should().NotBeNull();
        result.Entities.Should().HaveCount(3);
        result.Entities.Should().OnlyContain(u => !string.IsNullOrEmpty(u.Email));
    }

    public void Dispose()
    {
        _dbContext.Dispose();
    }
}
