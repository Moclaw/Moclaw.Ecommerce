namespace Ecom.Users.Application.Queries.Users;

public class GetUserPermissionsQueryHandler : IRequestHandler<GetUserPermissionsQuery, List<string>>
{
    private readonly IUserService _userService;
    private readonly ILogger<GetUserPermissionsQueryHandler> _logger;

    public GetUserPermissionsQueryHandler(IUserService userService, ILogger<GetUserPermissionsQueryHandler> logger)
    {
        _userService = userService;
        _logger = logger;
    }

    public async Task<List<string>> Handle(GetUserPermissionsQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Processing get user permissions query for user: {UserId}", request.UserId);
        
        var result = await _userService.GetUserPermissionsAsync(request.UserId, cancellationToken);
        
        _logger.LogInformation("User permissions retrieved for user: {UserId}, Count: {PermissionCount}", 
            request.UserId, result.Count);
        
        return result;
    }
}
