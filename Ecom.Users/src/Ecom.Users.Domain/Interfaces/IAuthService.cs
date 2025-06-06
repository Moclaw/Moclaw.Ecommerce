using Ecom.Users.Domain.DTOs;
using Ecom.Users.Domain.Entities;
using System.Security.Claims;

namespace Ecom.Users.Domain.Interfaces;

public interface IAuthService
{
    Task<Response<AuthResponseDto>> RegisterAsync(RegisterDto registerDto);
    Task<Response<AuthResponseDto>> LoginAsync(LoginDto loginDto);
    Task<Response<AuthResponseDto>> RefreshTokenAsync(RefreshTokenDto refreshTokenDto);
    Task<Response<AuthResponseDto>> HandleOAuthCallbackAsync(string provider, string code);
    Task<Response<bool>> RevokeAllRefreshTokensAsync(Guid userId);
    Task<Response<bool>> RevokeRefreshTokenAsync(string token);
    Task<Response<bool>> ValidateTokenAsync(string token);
    Task<(string AccessToken, string RefreshToken)> GenerateTokensAsync(User user);
    Task<User?> AuthenticateGoogleAsync(string googleToken);
    Task<User?> AuthenticateFacebookAsync(string facebookToken);
    Task<ClaimsPrincipal?> GetPrincipalFromTokenAsync(string token);
    Task<Response<AuthResponseDto>> SSOLoginAsync(string provider, string token);
    Task<Response<bool>> UpdateUserAsync(Guid currentUserId, Guid targetUserId, UpdateUserDto updateUserDto, bool isAdmin = false);
}
