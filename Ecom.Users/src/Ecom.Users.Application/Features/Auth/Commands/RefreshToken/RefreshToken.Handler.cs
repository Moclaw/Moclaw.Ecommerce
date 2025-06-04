using Ecom.Users.Domain.DTOs;
using Ecom.Users.Domain.Interfaces;

namespace Ecom.Users.Application.Features.Auth.Commands.RefreshToken
{
    public class RefreshTokenHandler(IAuthService authService)
        : ICommandHandler<RefreshTokenRequest, RefreshTokenResponse>
    {
        public async Task<Response<RefreshTokenResponse>> Handle(
            RefreshTokenRequest request,
            CancellationToken cancellationToken
        )
        {
            var refreshDto = new RefreshTokenDto { RefreshToken = request.RefreshToken };

            var result = await authService.RefreshTokenAsync(refreshDto);

            if (!result.IsSuccess || result.Data == null)
            {
                return new Response<RefreshTokenResponse>(
                    IsSuccess: false,
                    400,
                    result.Message ?? "Token refresh failed",
                    Data: null
                );
            }

            var authResponse = result.Data;
            var response = new RefreshTokenResponse
            {
                AccessToken = authResponse.AccessToken,
                RefreshToken = authResponse.RefreshToken,
                ExpiresAt = authResponse.ExpiresAt.DateTime, // Explicitly convert DateTimeOffset to DateTime
            };

            return new Response<RefreshTokenResponse>(
                IsSuccess: true,
                200,
                "Token refreshed successfully",
                Data: response
            );
        }
    }
}
