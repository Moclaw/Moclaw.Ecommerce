namespace Ecom.Users.Application.Queries.Users;

public class GetUserByIdQueryHandler : IRequestHandler<GetUserByIdQuery, UserDto?>
{
    private readonly IUserService _userService;
    private readonly ILogger<GetUserByIdQueryHandler> _logger;

    public GetUserByIdQueryHandler(IUserService userService, ILogger<GetUserByIdQueryHandler> logger)
    {
        _userService = userService;
        _logger = logger;
    }

    public async Task<UserDto?> Handle(GetUserByIdQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Processing get user by id query for user: {UserId}", request.UserId);
        
        var result = await _userService.GetByIdAsync(request.UserId, cancellationToken);
        
        if (result != null)
        {
            _logger.LogInformation("User found: {UserId}", request.UserId);
        }
        else
        {
            _logger.LogWarning("User not found: {UserId}", request.UserId);
        }
        
        return result;
    }
}
