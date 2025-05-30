namespace Ecom.Users.API.Endpoints.Auth;

public record RegisterRequest
{
    public string Email { get; init; } = string.Empty;
    public string Username { get; init; } = string.Empty;
    public string Password { get; init; } = string.Empty;
    public string FirstName { get; init; } = string.Empty;
    public string LastName { get; init; } = string.Empty;
    public string? PhoneNumber { get; init; }
}

[Summary("User registration")]
[Tags("Authentication")]
public class RegisterEndpoint : EndpointBase<RegisterRequest, Response<AuthResponse>>
{
    public override void Configure()
    {
        Post("/auth/register");
        AllowAnonymous();
        Version(1);
        Summary(s =>
        {
            s.Summary = "User registration";
            s.Description = "Registers a new user account";
            s.ExampleRequest = new RegisterRequest
            {
                Email = "user@example.com",
                Username = "username",
                Password = "password123",
                FirstName = "John",
                LastName = "Doe",
                PhoneNumber = "+1234567890"
            };
        });
    }

    public override async Task<Response<AuthResponse>> ExecuteAsync(RegisterRequest request, CancellationToken ct)
    {
        var command = new RegisterCommand
        {
            Email = request.Email,
            Username = request.Username,
            Password = request.Password,
            FirstName = request.FirstName,
            LastName = request.LastName,
            PhoneNumber = request.PhoneNumber,
            IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? string.Empty
        };

        return await new RegisterCommandHandler(Resolve<IAuthService>()).Handle(command, ct);
    }
}
