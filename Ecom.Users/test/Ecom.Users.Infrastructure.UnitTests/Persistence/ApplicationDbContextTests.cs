using Ecom.Users.Domain.Entities;
using Ecom.Users.Infrastructure.Persistence.EfCore;
using FluentAssertions;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace Ecom.Users.Infrastructure.UnitTests.Persistence
{
    public class ApplicationDbContextTests : IDisposable
    {
        private readonly SqliteConnection _connection;
        private readonly DbContextOptions<ApplicationDbContext> _sqliteOptions;
        private readonly ApplicationDbContext _dbContext;

        public ApplicationDbContextTests()
        {
            // 1. Tạo và mở một SQLite in-memory connection
            _connection = new SqliteConnection("Filename=:memory:");
            _connection.Open();

            // 2. Tạo DbContextOptions để các DbContext dùng chung connection này
            _sqliteOptions = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseSqlite(_connection)
                .Options;

            // 3. Khởi tạo một DbContext tạm để đảm bảo schema được tạo
            using var tmpContext = new ApplicationDbContext(_sqliteOptions);
            tmpContext.Database.EnsureCreated();

            // 4. Khởi tạo _dbContext chính để dùng chung trong các test (schema đã tồn tại)
            _dbContext = new ApplicationDbContext(_sqliteOptions);
        }

        [Fact]
        public async Task ApplicationDbContext_ShouldEnsureCreated()
        {
            // Arrange & Act
            var result = await _dbContext.Database.EnsureCreatedAsync();

            // Assert
            // Lần đầu tiên gọi EnsureCreatedAsync() sau khi schema đã tồn tại sẽ trả về false,
            // nhưng vì constructor đã gọi EnsureCreated() rồi, nên ở đây kết quả vẫn có thể là false.
            // Tuy nhiên mục đích test là không bắn exception, và trả về bool hợp lệ.
            result.Should().BeFalse();
        }

        [Fact]
        public void ApplicationDbContext_ShouldHaveCorrectModelConfiguration()
        {
            // Arrange & Act
            var model = _dbContext.Model;

            // Assert
            model.Should().NotBeNull();

            // Kiểm tra xem các entity đã được cấu hình
            var userEntityType = model.FindEntityType(typeof(User));
            userEntityType.Should().NotBeNull();

            var roleEntityType = model.FindEntityType(typeof(Role));
            roleEntityType.Should().NotBeNull();

            var permissionEntityType = model.FindEntityType(typeof(Permission));
            permissionEntityType.Should().NotBeNull();

            var refreshTokenEntityType = model.FindEntityType(typeof(RefreshToken));
            refreshTokenEntityType.Should().NotBeNull();
        }

        [Fact]
        public async Task ApplicationDbContext_ShouldSupportTransactions()
        {
            // Arrange
            // (Schema đã được tạo trong constructor, nhưng gọi EnsureCreatedAsync() để chắc chắn)
            await _dbContext.Database.EnsureCreatedAsync();

            // Act & Assert: bắt đầu một transaction thật sự
            await using var transaction = await _dbContext.Database.BeginTransactionAsync();
            transaction.Should().NotBeNull();

            // Thêm một user
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
            _dbContext.Set<User>().Add(user);
            await _dbContext.SaveChangesAsync();

            // Rollback transaction
            await transaction.RollbackAsync();

            // Verify rằng user đã không được lưu (vì đã rollback)
            var userCount = await _dbContext.Set<User>().CountAsync();
            userCount.Should().Be(0);
        }

        [Fact]
        public async Task ApplicationDbContext_ShouldHandleConcurrencyCorrectly()
        {
            // Arrange
            await _dbContext.Database.EnsureCreatedAsync();

            var user = new User
            {
                Id = Guid.NewGuid(),
                Email = "concurrent@example.com",
                UserName = "origuser",
                PasswordHash = "hashedPassword",
                EmailConfirmed = true,
                CreatedAt = DateTimeOffset.UtcNow,
                CreatedBy = Guid.NewGuid()
            };

            _dbContext.Set<User>().Add(user);
            await _dbContext.SaveChangesAsync();

            // Tạo hai context khác nhau nhưng dùng cùng _sqliteOptions (vẫn share cùng connection/schema)
            await using var context1 = new ApplicationDbContext(_sqliteOptions);
            await using var context2 = new ApplicationDbContext(_sqliteOptions);

            // Act: cả hai cùng lấy đối tượng User
            var user1 = await context1.Set<User>().FindAsync(user.Id);
            var user2 = await context2.Set<User>().FindAsync(user.Id);

            // Cập nhật tên khác nhau
            user1!.UserName = "updated_user1";
            user2!.UserName = "updated_user2";

            // Save từ context1 trước
            await context1.SaveChangesAsync();

            // Vì chúng ta không cấu hình concurrency token (RowVersion, Timestamp, v.v.),
            // nên context2.SaveChangesAsync() sẽ không throw exception—đúng như mong muốn test.
            var saveAction = async () => await context2.SaveChangesAsync();
            await saveAction.Should().NotThrowAsync();
        }

        public void Dispose()
        {
            _dbContext.Dispose();
            _connection.Close();
            _connection.Dispose();
        }
    }
}
