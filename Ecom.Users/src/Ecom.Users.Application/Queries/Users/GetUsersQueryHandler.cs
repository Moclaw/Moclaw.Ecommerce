namespace Ecom.Users.Application.Queries.Users;

public class GetUsersQueryHandler : IRequestHandler<GetUsersQuery, PagedResult<UserDto>>
{
    private readonly IUserService _userService;
    private readonly ILogger<GetUsersQueryHandler> _logger;

    public GetUsersQueryHandler(IUserService userService, ILogger<GetUsersQueryHandler> logger)
    {
        _userService = userService;
        _logger = logger;
    }

    public async Task<PagedResult<UserDto>> Handle(GetUsersQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Processing get users query - Page: {Page}, PageSize: {PageSize}, SearchTerm: {SearchTerm}", 
            request.Page, request.PageSize, request.SearchTerm);
        
        var result = await _userService.GetUsersAsync(request.Page, request.PageSize, request.SearchTerm, cancellationToken);
        
        _logger.LogInformation("Get users query completed - Total: {TotalCount}, Page: {Page}/{TotalPages}", 
            result.TotalCount, result.Page, result.TotalPages);
        
        return result;
    }
}
