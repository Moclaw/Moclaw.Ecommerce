namespace Ecom.Users.Application.Interfaces.Repositories;

public interface IUserRepository : IBaseRepository<User>
{
    Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);
    Task<User?> GetByUsernameAsync(string username, CancellationToken cancellationToken = default);
    Task<User?> GetByIdWithRolesAsync(int id, CancellationToken cancellationToken = default);
    Task<User?> GetByEmailWithRolesAsync(string email, CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(string email, string? username = null, CancellationToken cancellationToken = default);
    Task<PagedResult<User>> GetPagedAsync(int page, int pageSize, string? searchTerm = null, CancellationToken cancellationToken = default);
    Task<List<string>> GetUserPermissionsAsync(int userId, CancellationToken cancellationToken = default);
    Task<List<string>> GetUserRolesAsync(int userId, CancellationToken cancellationToken = default);
}
