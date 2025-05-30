namespace Ecom.Users.API.Endpoints.Auth;

public record RefreshTokenRequest
{
    public string RefreshToken { get; init; } = string.Empty;
}

[Summary("Refresh authentication token")]
[Tags("Authentication")]
public class RefreshTokenEndpoint : EndpointBase<RefreshTokenRequest, Response<AuthResponse>>
{
    public override void Configure()
    {
        Post("/auth/refresh");
        AllowAnonymous();
        Version(1);
        Summary(s =>
        {
            s.Summary = "Refresh authentication token";
            s.Description = "Refreshes an expired access token using a refresh token";
            s.ExampleRequest = new RefreshTokenRequest
            {
                RefreshToken = "your-refresh-token-here"
            };
        });
    }

    public override async Task<Response<AuthResponse>> ExecuteAsync(RefreshTokenRequest request, CancellationToken ct)
    {
        var command = new RefreshTokenCommand
        {
            RefreshToken = request.RefreshToken,
            IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? string.Empty
        };

        return await new RefreshTokenCommandHandler(Resolve<IAuthService>()).Handle(command, ct);
    }
}
