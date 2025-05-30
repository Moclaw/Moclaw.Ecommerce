using EfCore;

namespace Ecom.Users.Infrastructure.Persistence.EfCore.Repositories;

public class RoleRepository : BaseRepository<Role>, IRoleRepository
{
    public RoleRepository(UsersDbContext context) : base(context)
    {
    }

    public async Task<Role?> GetByNameAsync(string name, CancellationToken cancellationToken = default)
    {
        return await Context.Roles
            .FirstOrDefaultAsync(r => r.Name == name, cancellationToken);
    }

    public async Task<List<Role>> GetByUserIdAsync(int userId, CancellationToken cancellationToken = default)
    {
        return await Context.UserRoles
            .Where(ur => ur.UserId == userId && ur.IsActive && (ur.ExpiresAt == null || ur.ExpiresAt > DateTime.UtcNow))
            .Select(ur => ur.Role)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<Role>> GetAllWithPermissionsAsync(CancellationToken cancellationToken = default)
    {
        return await Context.Roles
            .Include(r => r.RolePermissions)
                .ThenInclude(rp => rp.Permission)
            .ToListAsync(cancellationToken);
    }

    public async Task<Role?> GetByIdWithPermissionsAsync(int id, CancellationToken cancellationToken = default)
    {
        return await Context.Roles
            .Include(r => r.RolePermissions)
                .ThenInclude(rp => rp.Permission)
            .FirstOrDefaultAsync(r => r.Id == id, cancellationToken);
    }
}
