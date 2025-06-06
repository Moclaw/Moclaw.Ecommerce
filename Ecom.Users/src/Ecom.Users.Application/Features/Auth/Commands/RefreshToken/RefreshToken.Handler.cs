using Ecom.Users.Domain.Constants;
using Ecom.Users.Domain.DTOs;
using Ecom.Users.Domain.Interfaces;
using Shared.Utils;

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
                return ResponseUtils.Error<RefreshTokenResponse>(
                    result.StatusCode,
                    result.Message ?? MessageKeys.Error);
            }

            var authResponse = result.Data;
            var response = new RefreshTokenResponse
            {
                AccessToken = authResponse.AccessToken,
                RefreshToken = authResponse.RefreshToken,
                ExpiresAt = authResponse.ExpiresAt.DateTime, // Explicitly convert DateTimeOffset to DateTime
            };

            return ResponseUtils.Success(response, MessageKeys.Success);
        }
    }
}
