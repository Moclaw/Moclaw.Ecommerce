using Ecom.Users.Application.Features.Auth.Commands.RefreshToken;

namespace Ecom.Users.API.Endpoints.Auth.Commands
{
    public class RefreshTokenEndpoint(IMediator mediator)
      : SingleEndpointBase<RefreshTokenRequest, RefreshTokenResponse>(mediator)
    {
        [HttpPost("auth/refresh")]
        public override async Task<Response<RefreshTokenResponse>> HandleAsync(
            RefreshTokenRequest req,
            CancellationToken ct
        )
        {
            return await _mediator.Send(req, ct);
        }
    }
}
