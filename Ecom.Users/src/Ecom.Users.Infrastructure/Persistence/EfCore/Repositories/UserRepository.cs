using EfCore;

namespace Ecom.Users.Infrastructure.Persistence.EfCore.Repositories;

public class UserRepository : BaseRepository<User>, IUserRepository
{
    public UserRepository(UsersDbContext context) : base(context)
    {
    }

    public async Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        return await Context.Users
            .FirstOrDefaultAsync(u => u.Email == email, cancellationToken);
    }

    public async Task<User?> GetByUsernameAsync(string username, CancellationToken cancellationToken = default)
    {
        return await Context.Users
            .FirstOrDefaultAsync(u => u.Username == username, cancellationToken);
    }

    public async Task<User?> GetByIdWithRolesAsync(int id, CancellationToken cancellationToken = default)
    {
        return await Context.Users
            .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
            .FirstOrDefaultAsync(u => u.Id == id, cancellationToken);
    }

    public async Task<User?> GetByEmailWithRolesAsync(string email, CancellationToken cancellationToken = default)
    {
        return await Context.Users
            .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
            .FirstOrDefaultAsync(u => u.Email == email, cancellationToken);
    }

    public async Task<bool> ExistsAsync(string email, string? username = null, CancellationToken cancellationToken = default)
    {
        var query = Context.Users.AsQueryable();
        
        if (!string.IsNullOrWhiteSpace(username))
        {
            return await query.AnyAsync(u => u.Email == email || u.Username == username, cancellationToken);
        }
        
        return await query.AnyAsync(u => u.Email == email, cancellationToken);
    }

    public async Task<PagedResult<User>> GetPagedAsync(int page, int pageSize, string? searchTerm = null, CancellationToken cancellationToken = default)
    {
        var query = Context.Users.AsQueryable();

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            query = query.Where(u => 
                u.Email.Contains(searchTerm) ||
                u.Username.Contains(searchTerm) ||
                u.FirstName.Contains(searchTerm) ||
                u.LastName.Contains(searchTerm));
        }

        var totalCount = await query.CountAsync(cancellationToken);
        
        var items = await query
            .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
            .OrderBy(u => u.Email)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return new PagedResult<User>
        {
            Items = items,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }

    public async Task<List<string>> GetUserPermissionsAsync(int userId, CancellationToken cancellationToken = default)
    {
        return await Context.Users
            .Where(u => u.Id == userId)
            .SelectMany(u => u.UserRoles)
            .Where(ur => ur.IsActive && (ur.ExpiresAt == null || ur.ExpiresAt > DateTime.UtcNow))
            .SelectMany(ur => ur.Role.RolePermissions)
            .Where(rp => rp.IsActive)
            .Select(rp => rp.Permission.Name)
            .Distinct()
            .ToListAsync(cancellationToken);
    }

    public async Task<List<string>> GetUserRolesAsync(int userId, CancellationToken cancellationToken = default)
    {
        return await Context.Users
            .Where(u => u.Id == userId)
            .SelectMany(u => u.UserRoles)
            .Where(ur => ur.IsActive && (ur.ExpiresAt == null || ur.ExpiresAt > DateTime.UtcNow))
            .Select(ur => ur.Role.Name)
            .ToListAsync(cancellationToken);
    }
}
