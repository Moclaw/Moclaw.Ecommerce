using Ecom.Users.Application.Features.Auth.Commands.Login;

namespace Ecom.Users.API.Endpoints.Auth.Commands
{
    [OpenApiSummary("Login user and return JWT token")]
    [OpenApiResponse(200, ResponseType = typeof(LoginResponse), Description = "Login successful")]
    public class LoginEndpoint(IMediator mediator)
        : SingleEndpointBase<LoginRequest, LoginResponse>(mediator)
    {
        [HttpPost("auth/login")]
        public override async Task<Response<LoginResponse>> HandleAsync(
            LoginRequest req,
            CancellationToken ct
        )
        {
            return await _mediator.Send(req, ct);
        }
    }
}
