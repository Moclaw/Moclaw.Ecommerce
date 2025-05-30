namespace Ecom.Users.API.Endpoints.Auth;

public record LoginRequest
{
    public string Email { get; init; } = string.Empty;
    public string Password { get; init; } = string.Empty;
    public bool RememberMe { get; init; } = false;
}

[Summary("User login")]
[Tags("Authentication")]
public class LoginEndpoint : EndpointBase<LoginRequest, Response<AuthResponse>>
{
    public override void Configure()
    {
        Post("/auth/login");
        AllowAnonymous();
        Version(1);
        Summary(s =>
        {
            s.Summary = "User login";
            s.Description = "Authenticates a user with email and password";
            s.ExampleRequest = new LoginRequest
            {
                Email = "user@example.com",
                Password = "password123",
                RememberMe = true
            };
        });
    }

    public override async Task<Response<AuthResponse>> ExecuteAsync(LoginRequest request, CancellationToken ct)
    {
        var command = new LoginCommand
        {
            Email = request.Email,
            Password = request.Password,
            RememberMe = request.RememberMe,
            IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? string.Empty
        };

        return await new LoginCommandHandler(Resolve<IAuthService>()).Handle(command, ct);
    }
}
