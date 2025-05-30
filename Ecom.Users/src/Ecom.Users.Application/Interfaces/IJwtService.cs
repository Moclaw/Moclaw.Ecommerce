namespace Ecom.Users.Application.Interfaces;

public interface IJwtService
{
    string GenerateAccessToken(User user, IList<string> roles, IList<string> permissions);
    string GenerateRefreshToken();
    bool ValidateToken(string token);
    Guid? GetUserIdFromToken(string token);
}
