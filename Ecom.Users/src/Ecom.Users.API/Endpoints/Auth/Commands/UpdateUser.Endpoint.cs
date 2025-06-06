using Ecom.Users.Application.Features.Auth.Commands.UpdateUser;

namespace Ecom.Users.API.Endpoints.Auth.Commands
{
    public class UpdateUserEndpoint(IMediator mediator)
        : SingleEndpointBase<UpdateUserRequest, UpdateUserResponse>(mediator)
    {
        [HttpPut("auth/users/{userId}")]
        public override async Task<Response<UpdateUserResponse>> HandleAsync(
            UpdateUserRequest req,
            CancellationToken ct
        )
        {
            return await _mediator.Send(req, ct);
        }
    }
}
