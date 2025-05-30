namespace Ecom.Users.Application.Commands.Users;

public class UpdateUserCommandHandler : IRequestHandler<UpdateUserCommand, UserDto>
{
    private readonly IUserService _userService;
    private readonly ILogger<UpdateUserCommandHandler> _logger;

    public UpdateUserCommandHandler(IUserService userService, ILogger<UpdateUserCommandHandler> logger)
    {
        _userService = userService;
        _logger = logger;
    }

    public async Task<UserDto> Handle(UpdateUserCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Processing update user request for user: {UserId}", request.UserId);
        
        var updateUserDto = new UpdateUserDto
        {
            Email = request.Email,
            Username = request.Username,
            FirstName = request.FirstName,
            LastName = request.LastName,
            PhoneNumber = request.PhoneNumber
        };

        var result = await _userService.UpdateAsync(request.UserId, updateUserDto, cancellationToken);
        
        _logger.LogInformation("User update successful for user: {UserId}", request.UserId);
        
        return result;
    }
}
