namespace Ecom.Users.Application.Commands.Auth;

public class RegisterCommandHandler : IRequestHandler<RegisterCommand, AuthResultDto>
{
    private readonly IAuthService _authService;
    private readonly ILogger<RegisterCommandHandler> _logger;

    public RegisterCommandHandler(IAuthService authService, ILogger<RegisterCommandHandler> logger)
    {
        _authService = authService;
        _logger = logger;
    }

    public async Task<AuthResultDto> Handle(RegisterCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Processing registration request for email: {Email}", request.Email);
        
        var registerDto = new RegisterDto
        {
            Email = request.Email,
            Username = request.Username,
            Password = request.Password,
            FirstName = request.FirstName,
            LastName = request.LastName,
            PhoneNumber = request.PhoneNumber,
            DeviceInfo = request.DeviceInfo,
            IpAddress = request.IpAddress
        };

        var result = await _authService.RegisterAsync(registerDto, cancellationToken);
        
        _logger.LogInformation("Registration successful for user: {UserId}", result.User.Id);
        
        return result;
    }
}
