namespace Ecom.Users.Application.Commands.Auth;

public class SocialLoginCommandHandler : IRequestHandler<SocialLoginCommand, AuthResultDto>
{
    private readonly IAuthService _authService;
    private readonly ILogger<SocialLoginCommandHandler> _logger;

    public SocialLoginCommandHandler(IAuthService authService, ILogger<SocialLoginCommandHandler> logger)
    {
        _authService = authService;
        _logger = logger;
    }

    public async Task<AuthResultDto> Handle(SocialLoginCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Processing social login request for provider: {Provider}", request.Provider);
        
        var socialLoginDto = new SocialLoginDto
        {
            Provider = request.Provider,
            Token = request.Token,
            Email = request.Email,
            FirstName = request.FirstName,
            LastName = request.LastName,
            ProfilePictureUrl = request.ProfilePictureUrl,
            DeviceInfo = request.DeviceInfo,
            IpAddress = request.IpAddress
        };

        var result = await _authService.SocialLoginAsync(socialLoginDto, cancellationToken);
        
        _logger.LogInformation("Social login successful for user: {UserId} via {Provider}", result.User.Id, request.Provider);
        
        return result;
    }
}
