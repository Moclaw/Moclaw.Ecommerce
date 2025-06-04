using Ecom.Users.Domain.DTOs;
using Ecom.Users.Domain.Interfaces;

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
            var registerDto = new RegisterDto
            {
                Email = request.Email,
                Password = request.Password,
                ConfirmPassword = request.ConfirmPassword,
                FirstName = request.FirstName ?? "",
                LastName = request.LastName ?? "",
                PhoneNumber = request.PhoneNumber
            };

            var result = await authService.RegisterAsync(registerDto);
            
            if (!result.IsSuccess || result.Data == null)
            {
                return new Response<RegisterResponse>(
                    IsSuccess: false,
                    400,
                    result.Message ?? "Registration failed",
                    Data: null
                );
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

            return new Response<RegisterResponse>(
                IsSuccess: true,
                201,
                "Registration successful",
                Data: response
            );
        }
    }
}
