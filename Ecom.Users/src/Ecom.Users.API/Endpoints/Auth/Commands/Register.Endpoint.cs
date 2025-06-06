using Ecom.Users.Application.Features.Auth.Commands.Register;

namespace Ecom.Users.API.Endpoints.Auth.Commands
{
    public class RegisterEndpoint(IMediator mediator)
        : SingleEndpointBase<RegisterRequest, RegisterResponse>(mediator)
    {
        [HttpPost("auth/register")]
        public override async Task<Response<RegisterResponse>> HandleAsync(
            RegisterRequest req,
            CancellationToken ct
        )
        {
            return await _mediator.Send(req, ct);
        }
    }
}
