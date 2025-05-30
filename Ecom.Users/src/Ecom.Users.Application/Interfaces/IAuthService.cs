using Ecom.Users.Application.DTOs.Auth;

namespace Ecom.Users.Application.Interfaces;

public interface IAuthService
{
    Task<Response<AuthResponse>> LoginAsync(LoginRequest request, string ipAddress);
    Task<Response<AuthResponse>> RegisterAsync(RegisterRequest request, string ipAddress);
    Task<Response<AuthResponse>> RefreshTokenAsync(RefreshTokenRequest request, string ipAddress);
    Task<Response<bool>> RevokeRefreshTokenAsync(string token, string ipAddress);
    Task<Response<AuthResponse>> SocialLoginAsync(SocialLoginRequest request, string ipAddress);
}
