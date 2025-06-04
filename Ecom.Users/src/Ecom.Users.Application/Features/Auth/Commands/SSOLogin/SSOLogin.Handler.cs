using Ecom.Users.Domain.Interfaces;

namespace Ecom.Users.Application.Features.Auth.Commands.SSOLogin
{
    public class SSOLoginHandler(
        IAuthService authService
    ) : ICommandHandler<SSOLoginRequest, SSOLoginResponse>
    {
        public async Task<Response<SSOLoginResponse>> Handle(
            SSOLoginRequest request,
            CancellationToken cancellationToken
        )
        {
            var result = await authService.SSOLoginAsync(request.Provider, request.Token);
            
            if (!result.IsSuccess || result.Data == null)
            {
                return new Response<SSOLoginResponse>(
                    IsSuccess: false,
                    400,
                    result.Message ?? "SSO login failed",
                    Data: null
                );
            }

            var authResponse = result.Data;
            var response = new SSOLoginResponse
            {
                UserId = authResponse.UserId,
                Email = authResponse.Email,
                AccessToken = authResponse.AccessToken,
                RefreshToken = authResponse.RefreshToken,
                ExpiresAt = authResponse.ExpiresAt.DateTime,
                Roles = [.. authResponse.Roles],
                Provider = request.Provider
            };

            return new Response<SSOLoginResponse>(
                IsSuccess: true,
                200,
                "SSO login successful",
                Data: response
            );
        }
    }
}
