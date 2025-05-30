using EfCore;

namespace Ecom.Users.Infrastructure.Persistence.EfCore.Repositories;

public class RefreshTokenRepository : BaseRepository<RefreshToken>, IRefreshTokenRepository
{
    public RefreshTokenRepository(UsersDbContext context) : base(context)
    {
    }

    public async Task<RefreshToken?> GetByTokenAsync(string token, CancellationToken cancellationToken = default)
    {
        return await Context.RefreshTokens
            .Include(rt => rt.User)
            .FirstOrDefaultAsync(rt => rt.Token == token, cancellationToken);
    }

    public async Task<RefreshToken?> GetByJwtIdAsync(string jwtId, CancellationToken cancellationToken = default)
    {
        return await Context.RefreshTokens
            .Include(rt => rt.User)
            .FirstOrDefaultAsync(rt => rt.JwtId == jwtId, cancellationToken);
    }

    public async Task<List<RefreshToken>> GetByUserIdAsync(int userId, CancellationToken cancellationToken = default)
    {
        return await Context.RefreshTokens
            .Where(rt => rt.UserId == userId)
            .OrderByDescending(rt => rt.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task InvalidateAllUserTokensAsync(int userId, CancellationToken cancellationToken = default)
    {
        var tokens = await Context.RefreshTokens
            .Where(rt => rt.UserId == userId && !rt.Invalidated)
            .ToListAsync(cancellationToken);

        foreach (var token in tokens)
        {
            token.Invalidated = true;
            token.InvalidatedAt = DateTime.UtcNow;
        }

        await Context.SaveChangesAsync(cancellationToken);
    }

    public async Task InvalidateTokenAsync(string token, CancellationToken cancellationToken = default)
    {
        var refreshToken = await Context.RefreshTokens
            .FirstOrDefaultAsync(rt => rt.Token == token, cancellationToken);

        if (refreshToken != null)
        {
            refreshToken.Invalidated = true;
            refreshToken.InvalidatedAt = DateTime.UtcNow;
            await Context.SaveChangesAsync(cancellationToken);
        }
    }

    public async Task CleanupExpiredTokensAsync(CancellationToken cancellationToken = default)
    {
        var expiredTokens = await Context.RefreshTokens
            .Where(rt => rt.ExpiresAt < DateTime.UtcNow)
            .ToListAsync(cancellationToken);

        Context.RefreshTokens.RemoveRange(expiredTokens);
        await Context.SaveChangesAsync(cancellationToken);
    }
}
