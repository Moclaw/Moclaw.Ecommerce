using Ecom.Users.Domain.Constants;
using Ecom.Users.Domain.DTOs;
using Ecom.Users.Domain.Interfaces;
using Shared.Utils;

namespace Ecom.Users.Application.Features.Auth.Commands.Register
{
    public class RegisterHandler(
        IAuthService authService
    ) : ICommandHandler<RegisterRequest, RegisterResponse>
    {
        public async Task<Response<RegisterResponse>> Handle(
            RegisterRequest request,
            CancellationToken cancellationToken
        )
        {
            // Validate request
            if (string.IsNullOrEmpty(request.Email))
            {
                return ResponseUtils.Error<RegisterResponse>(400, "Email is required");
            }

            if (string.IsNullOrEmpty(request.Password))
            {
                return ResponseUtils.Error<RegisterResponse>(400, "Password is required");
            }

            var registerDto = new RegisterDto
            {
                Email = request.Email,
                Password = request.Password,
                ConfirmPassword = request.ConfirmPassword,
                FirstName = request.FirstName ?? "",
                LastName = request.LastName ?? "",
                PhoneNumber = request.PhoneNumber,
                UserName = request.UserName ?? ""
            };

            var result = await authService.RegisterAsync(registerDto);
            
            if (!result.IsSuccess || result.Data == null)
            {
                return ResponseUtils.Error<RegisterResponse>(
                    result.StatusCode,
                    result.Message ?? MessageKeys.Error);
            }

            var authResponse = result.Data;
            var response = new RegisterResponse
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
