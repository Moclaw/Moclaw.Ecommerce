using Ecom.Users.Domain.Constants;
using Ecom.Users.Domain.Interfaces;
using Shared.Utils;

namespace Ecom.Users.Application.Features.Users.Queries.GetById
{
    public class GetByIdHandler(
        IUserService userService
    ) : IQueryHandler<GetByIdRequest, GetByIdResponse>
    {
        public async Task<Response<GetByIdResponse>> Handle(
            GetByIdRequest request,
            CancellationToken cancellationToken
        )
        {
            var userResult = await userService.GetUserByIdAsync(request.Id);

            if (!userResult.IsSuccess || userResult.Data == null)
            {
                return ResponseUtils.Error<GetByIdResponse>(
                    400,
                   message: userResult.Message ?? MessageKeys.UserNotFound);
            }

            var user = userResult.Data;
            var rolesResult = await userService.GetUserRolesAsync(request.Id);

            var response = new GetByIdResponse
            {
                Id = user.Id,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                PhoneNumber = user.PhoneNumber,
                IsActive = user.TwoFactorEnabled,
                CreatedAt = user.CreatedAt,
                UpdatedAt = user.UpdatedAt,
                Roles = rolesResult.IsSuccess ? rolesResult.Data?.Select(r => r.Name).ToList() ?? [] : []
            };

            return ResponseUtils.Success(response, MessageKeys.Success);
        }
    }
}
