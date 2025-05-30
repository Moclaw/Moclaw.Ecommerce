namespace Ecom.Users.Application.Commands.Users;

public class ChangePasswordCommandHandler : IRequestHandler<ChangePasswordCommand, bool>
{
    private readonly IUserService _userService;
    private readonly ILogger<ChangePasswordCommandHandler> _logger;

    public ChangePasswordCommandHandler(IUserService userService, ILogger<ChangePasswordCommandHandler> logger)
    {
        _userService = userService;
        _logger = logger;
    }

    public async Task<bool> Handle(ChangePasswordCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Processing change password request for user: {UserId}", request.UserId);
        
        var changePasswordDto = new ChangePasswordDto
        {
            CurrentPassword = request.CurrentPassword,
            NewPassword = request.NewPassword
        };

        var result = await _userService.ChangePasswordAsync(request.UserId, changePasswordDto, cancellationToken);
        
        _logger.LogInformation("Password change successful for user: {UserId}", request.UserId);
        
        return result;
    }
}
