namespace Ecom.Users.Application.Interfaces.Repositories;

public interface IRoleRepository : IBaseRepository<Role>
{
    Task<Role?> GetByNameAsync(string name, CancellationToken cancellationToken = default);
    Task<List<Role>> GetByUserIdAsync(int userId, CancellationToken cancellationToken = default);
    Task<List<Role>> GetAllWithPermissionsAsync(CancellationToken cancellationToken = default);
    Task<Role?> GetByIdWithPermissionsAsync(int id, CancellationToken cancellationToken = default);
}
