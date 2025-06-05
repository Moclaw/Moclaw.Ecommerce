using Ecom.Users.Domain.Constants;
using Ecom.Users.Domain.Interfaces;
using Shared.Utils;

namespace Ecom.Users.Application.Features.Permissions.Queries.ValidatePermission
{
    public class ValidatePermissionHandler(
        IUserService userService
    ) : IQueryHandler<ValidatePermissionRequest, ValidatePermissionResponse>
    {
        public async Task<Response<ValidatePermissionResponse>> Handle(
            ValidatePermissionRequest request,
            CancellationToken cancellationToken
        )
        {
            var userResult = await userService.GetUserByIdAsync(request.UserId);
            if (!userResult.IsSuccess || userResult.Data == null)
            {
                return ResponseUtils.Error<ValidatePermissionResponse>(
                    404,
                    MessageKeys.UserNotFound);
            }

            var rolesResult = await userService.GetUserRolesAsync(request.UserId);
            var roles = rolesResult.IsSuccess ? rolesResult.Data?.Select(r => r.Name).ToList() ?? [] : [];

            bool hasPermission = false;
            string reason = "";

            // Admin check
            if (roles.Contains("Admin"))
            {
                hasPermission = true;
                reason = "Admin role";
            }
            // Ownership check for user resource
            else if (request.Resource.Equals("user", StringComparison.OrdinalIgnoreCase))
            {
                if (request.ResourceId.HasValue && request.ResourceId.Value == request.UserId)
                {
                    hasPermission = true;
                    reason = "Resource ownership";
                }
                else if (!request.ResourceId.HasValue && request.Action.Equals("read", StringComparison.OrdinalIgnoreCase))
                {
                    hasPermission = true;
                    reason = "Self read access";
                }
                else
                {
                    reason = MessageKeys.AccessDenied;
                }
            }
            else
            {
                reason = "Resource not supported";
            }

            var response = new ValidatePermissionResponse
            {
                HasPermission = hasPermission,
                Action = request.Action,
                Resource = request.Resource,
                UserId = request.UserId,
                Roles = roles,
                Reason = reason
            };

            return ResponseUtils.Success(response, MessageKeys.Success);
        }
    }
}
