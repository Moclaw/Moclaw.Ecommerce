namespace Ecom.Users.Application.Commands.Auth;

public class RefreshTokenCommandHandler : IRequestHandler<RefreshTokenCommand, AuthResultDto>
{
    private readonly IAuthService _authService;
    private readonly ILogger<RefreshTokenCommandHandler> _logger;

    public RefreshTokenCommandHandler(IAuthService authService, ILogger<RefreshTokenCommandHandler> logger)
    {
        _authService = authService;
        _logger = logger;
    }

    public async Task<AuthResultDto> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Processing refresh token request");
        
        var refreshTokenDto = new RefreshTokenDto
        {
            RefreshToken = request.RefreshToken,
            AccessToken = request.AccessToken,
            JwtId = request.JwtId,
            DeviceInfo = request.DeviceInfo,
            IpAddress = request.IpAddress
        };

        var result = await _authService.RefreshTokenAsync(refreshTokenDto, cancellationToken);
        
        _logger.LogInformation("Token refresh successful for user: {UserId}", result.User.Id);
        
        return result;
    }
}
