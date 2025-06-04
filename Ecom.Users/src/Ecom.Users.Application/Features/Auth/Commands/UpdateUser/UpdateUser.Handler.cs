using Ecom.Users.Domain.DTOs;
using Ecom.Users.Domain.Interfaces;

namespace Ecom.Users.Application.Features.Auth.Commands.UpdateUser
{
    public class UpdateUserHandler(
        IAuthService authService
    ) : ICommandHandler<UpdateUserRequest, UpdateUserResponse>
    {
        public async Task<Response<UpdateUserResponse>> Handle(
            UpdateUserRequest request,
            CancellationToken cancellationToken
        )
        {
            var updateDto = new UpdateUserDto
            {
                Email = request.Email,
                UserName = request.UserName,
                FirstName = request.FirstName,
                LastName = request.LastName,
                PhoneNumber = request.PhoneNumber,
                CurrentPassword = request.CurrentPassword,
                NewPassword = request.NewPassword
            };

            var result = await authService.UpdateUserAsync(
                request.CurrentUserId, 
                request.TargetUserId, 
                updateDto, 
                request.IsAdmin);

            var response = new UpdateUserResponse
            {
                IsSuccess = result.IsSuccess,
                UserId = request.TargetUserId
            };

            return new Response<UpdateUserResponse>(
                IsSuccess: result.IsSuccess,
                result.IsSuccess ? 200 : 400,
                result.Message ?? (result.IsSuccess ? "User updated successfully" : "User update failed"),
                Data: response
            );
        }
    }
}
