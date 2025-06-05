using Ecom.Users.Domain.Constants;
using Ecom.Users.Domain.Interfaces;
using Shared.Utils;

namespace Ecom.Users.Application.Features.Users.Queries.GetAll
{
    public class GetAllHandler(
         IUserService userService
    ) : IQueryCollectionHandler<GetAllRequest, GetAllResponse>
    {
        public async Task<ResponseCollection<GetAllResponse>> Handle(
            GetAllRequest request,
            CancellationToken cancellationToken
        )
        {
            var result = await userService.GetUsersAsync(
                request.PageIndex,
                request.PageSize,
                request.Search
            );

            if (!result.IsSuccess || result.Data == null)
            {
                return new ResponseCollection<GetAllResponse>(
                    IsSuccess: false,
                    400,
                    result.Message ?? MessageKeys.Error,
                    Data: []
                );
            }

            var users = result.Data.Select(user => new GetAllResponse
            {
                Id = user.Id,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                IsActive = user.TwoFactorEnabled,
                CreatedAt = user.CreatedAt.DateTime,
                Roles = [] // TODO: Load roles efficiently
            }).ToList();

            return new ResponseCollection<GetAllResponse>(
                IsSuccess: true,
                200,
                MessageKeys.Success,
                Data: users
            );
        }
    }
}
