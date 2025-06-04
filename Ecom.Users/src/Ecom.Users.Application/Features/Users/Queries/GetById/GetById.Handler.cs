using Ecom.Users.Domain.Interfaces;

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
                return new Response<GetByIdResponse>(
                    IsSuccess: false,
                    404,
                    "User not found",
                    Data: null
                );
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
                CreatedAt = user.CreatedAt.DateTime,
                UpdatedAt = user.UpdatedAt.Value.DateTime,
                Roles = rolesResult.IsSuccess ? rolesResult.Data?.Select(r => r.Name).ToList() ?? [] : []
            };

            return new Response<GetByIdResponse>(
                IsSuccess: true,
                200,
                "User retrieved successfully",
                Data: response
            );
        }
    }
}
