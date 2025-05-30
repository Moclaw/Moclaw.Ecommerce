namespace Ecom.Users.Application.Commands.Auth;

public record SocialLoginCommand : IRequest<Response<AuthResponse>>
{
    public string Provider { get; init; } = string.Empty;
    public string Token { get; init; } = string.Empty;
    public string? Email { get; init; }
    public string? FirstName { get; init; }
    public string? LastName { get; init; }
    public string? ExternalId { get; init; }
    public string IpAddress { get; init; } = string.Empty;
}

public class SocialLoginCommandHandler : IRequestHandler<SocialLoginCommand, Response<AuthResponse>>
{
    private readonly IAuthService _authService;

    public SocialLoginCommandHandler(IAuthService authService)
    {
        _authService = authService;
    }

    public async Task<Response<AuthResponse>> Handle(SocialLoginCommand request, CancellationToken cancellationToken)
    {
        var socialLoginRequest = new SocialLoginRequest
        {
            Provider = request.Provider,
            Token = request.Token,
            Email = request.Email,
            FirstName = request.FirstName,
            LastName = request.LastName,
            ExternalId = request.ExternalId
        };

        return await _authService.SocialLoginAsync(socialLoginRequest, request.IpAddress);
    }
}
