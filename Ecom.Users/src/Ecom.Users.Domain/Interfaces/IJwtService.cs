using Ecom.Users.Domain.Entities;
using Ecom.Users.Domain.ValueObjects;
using System.Security.Claims;

namespace Ecom.Users.Domain.Interfaces;

public interface IJwtService
{
    string GenerateAccessToken(User user, IEnumerable<string> permissions);
    RefreshToken GenerateRefreshToken(User user, string jwtId);
    ClaimsPrincipal? GetPrincipalFromExpiredToken(string token);
    bool ValidateToken(string token);
    string GetJwtId(string token);
}
