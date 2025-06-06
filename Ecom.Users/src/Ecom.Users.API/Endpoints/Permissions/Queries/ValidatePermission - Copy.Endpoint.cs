using Ecom.Users.Application.Features.Permissions.Queries.ValidatePermission;

namespace Ecom.Users.API.Endpoints.Permissions.Queries
{
    public class ValidatePermission2Endpoint(IMediator mediator)
        : SingleEndpointBase<ValidatePermissionRequest, ValidatePermissionResponse>(mediator)
    {
        [HttpGet("permissions/validate2")]
        public override async Task<Response<ValidatePermissionResponse>> HandleAsync(
            ValidatePermissionRequest req,
            CancellationToken ct
        )
        {
            return await _mediator.Send(req, ct);
        }
    }
}
