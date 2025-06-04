using Ecom.Users.Application.Features.Auth.Commands.SSOLogin;

namespace Ecom.Users.API.Endpoints.Auth.Commands
{
    public class SSOLoginEndpoint(IMediator mediator)
        : SingleEndpointBase<SSOLoginRequest, SSOLoginResponse>(mediator)
    {
        [HttpPost("auth/sso-login")]
        public override async Task<Response<SSOLoginResponse>> HandleAsync(
            SSOLoginRequest req,
            CancellationToken ct
        )
        {
            return await _mediator.Send(req, ct);
        }
    }
}
