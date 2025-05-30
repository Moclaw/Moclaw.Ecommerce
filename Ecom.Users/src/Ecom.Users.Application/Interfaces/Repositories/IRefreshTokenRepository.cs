namespace Ecom.Users.Application.Interfaces.Repositories;

public interface IRefreshTokenRepository : IBaseRepository<RefreshToken>
{
    Task<RefreshToken?> GetByTokenAsync(string token, CancellationToken cancellationToken = default);
    Task<RefreshToken?> GetByJwtIdAsync(string jwtId, CancellationToken cancellationToken = default);
    Task<List<RefreshToken>> GetByUserIdAsync(int userId, CancellationToken cancellationToken = default);
    Task InvalidateAllUserTokensAsync(int userId, CancellationToken cancellationToken = default);
    Task InvalidateTokenAsync(string token, CancellationToken cancellationToken = default);
    Task CleanupExpiredTokensAsync(CancellationToken cancellationToken = default);
}
