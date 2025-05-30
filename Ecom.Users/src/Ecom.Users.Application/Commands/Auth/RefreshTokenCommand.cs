namespace Ecom.Users.Application.Commands.Auth;

public record RefreshTokenCommand : IRequest<Response<AuthResponse>>
{
    public string RefreshToken { get; init; } = string.Empty;
    public string IpAddress { get; init; } = string.Empty;
}

public class RefreshTokenCommandHandler : IRequestHandler<RefreshTokenCommand, Response<AuthResponse>>
{
    private readonly IAuthService _authService;

    public RefreshTokenCommandHandler(IAuthService authService)
    {
        _authService = authService;
    }

    public async Task<Response<AuthResponse>> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
    {
        var refreshRequest = new RefreshTokenRequest
        {
            RefreshToken = request.RefreshToken
        };

        return await _authService.RefreshTokenAsync(refreshRequest, request.IpAddress);
    }
}
