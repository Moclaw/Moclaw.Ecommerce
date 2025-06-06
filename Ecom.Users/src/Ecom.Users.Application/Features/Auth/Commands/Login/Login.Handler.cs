using Ecom.Users.Domain.Constants;
using Ecom.Users.Domain.DTOs;
using Ecom.Users.Domain.Interfaces;
using Shared.Utils;

namespace Ecom.Users.Application.Features.Auth.Commands.Login
{
    public class LoginHandler(
            IAuthService authService
        ) : ICommandHandler<LoginRequest, LoginResponse>
    {
        public async Task<Response<LoginResponse>> Handle(LoginRequest request, CancellationToken cancellationToken)
        {
            var loginDto = new LoginDto
            {
                Email = request.Email ?? string.Empty,
                Password = request.Password ?? string.Empty,
                RememberMe = request.RememberMe
            };

            var result = await authService.LoginAsync(loginDto);

            if (!result.IsSuccess || result.Data == null)
            {
                return ResponseUtils.Error<LoginResponse>(
                    result.StatusCode,
                    result.Message ?? MessageKeys.InvalidCredentials);
            }

            var authResponse = result.Data;
            var response = new LoginResponse
            {
                UserId = authResponse.UserId,
                Email = authResponse.Email,
                AccessToken = authResponse.AccessToken,
                RefreshToken = authResponse.RefreshToken,
                ExpiresAt = authResponse.ExpiresAt.DateTime,
                Roles = [.. authResponse.Roles]
            };

            return ResponseUtils.Success(response, MessageKeys.Success);
        }
    }
}