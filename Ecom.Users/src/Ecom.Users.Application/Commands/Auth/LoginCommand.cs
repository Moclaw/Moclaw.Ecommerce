namespace Ecom.Users.Application.Commands.Auth;

public record LoginCommand : IRequest<Response<AuthResponse>>
{
    public string Email { get; init; } = string.Empty;
    public string Password { get; init; } = string.Empty;
    public bool RememberMe { get; init; } = false;
    public string IpAddress { get; init; } = string.Empty;
}

public class LoginCommandHandler : IRequestHandler<LoginCommand, Response<AuthResponse>>
{
    private readonly IAuthService _authService;

    public LoginCommandHandler(IAuthService authService)
    {
        _authService = authService;
    }

    public async Task<Response<AuthResponse>> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        var loginRequest = new LoginRequest
        {
            Email = request.Email,
            Password = request.Password,
            RememberMe = request.RememberMe
        };

        return await _authService.LoginAsync(loginRequest, request.IpAddress);
    }
}
