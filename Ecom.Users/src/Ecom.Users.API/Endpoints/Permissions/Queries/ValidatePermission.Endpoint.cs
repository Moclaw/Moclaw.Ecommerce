using Ecom.Users.Application.Features.Permissions.Queries.ValidatePermission;

namespace Ecom.Users.API.Endpoints.Permissions.Queries
{
    public class ValidatePermissionEndpoint(IMediator mediator)
        : SingleEndpointBase<ValidatePermissionRequest, ValidatePermissionResponse>(mediator)
    {
        [HttpGet("permissions/validate")]
        public override async Task<Response<ValidatePermissionResponse>> HandleAsync(
            ValidatePermissionRequest req,
            CancellationToken ct
        )
        {
            return await _mediator.Send(req, ct);
        }
    }
}
