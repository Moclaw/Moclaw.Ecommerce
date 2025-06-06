using Ecom.Users.Domain.Constants;
using Ecom.Users.Domain.DTOs;
using Ecom.Users.Domain.Interfaces;
using Shared.Utils;

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

            if (!result.IsSuccess)
            {
                return ResponseUtils.Error<UpdateUserResponse>(
                    result.StatusCode,
                    result.Message ?? MessageKeys.Error);
            }

            var response = new UpdateUserResponse
            {
                IsSuccess = result.IsSuccess,
                UserId = request.TargetUserId
            };

            return ResponseUtils.Success(
                response, 
                result.Message ?? MessageKeys.Success);
        }
    }
}
