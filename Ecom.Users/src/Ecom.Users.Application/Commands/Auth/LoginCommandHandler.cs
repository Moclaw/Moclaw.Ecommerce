namespace Ecom.Users.Application.Commands.Auth;

public class LoginCommandHandler : IRequestHandler<LoginCommand, AuthResultDto>
{
    private readonly IAuthService _authService;
    private readonly ILogger<LoginCommandHandler> _logger;

    public LoginCommandHandler(IAuthService authService, ILogger<LoginCommandHandler> logger)
    {
        _authService = authService;
        _logger = logger;
    }

    public async Task<AuthResultDto> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Processing login request for email: {Email}", request.Email);
        
        var loginDto = new LoginDto
        {
            Email = request.Email,
            Password = request.Password,
            DeviceInfo = request.DeviceInfo,
            IpAddress = request.IpAddress
        };

        var result = await _authService.LoginAsync(loginDto, cancellationToken);
        
        _logger.LogInformation("Login successful for user: {UserId}", result.User.Id);
        
        return result;
    }
}
