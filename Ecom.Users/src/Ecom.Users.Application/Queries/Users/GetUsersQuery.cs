namespace Ecom.Users.Application.Queries.Users;

public record GetUsersQuery : IRequest<Response<PaginatedList<UserDto>>>
{
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 10;
    public string? Search { get; init; }
    public string? Role { get; init; }
    public bool? EmailConfirmed { get; init; }
}

public class GetUsersQueryHandler : IRequestHandler<GetUsersQuery, Response<PaginatedList<UserDto>>>
{
    private readonly IUserService _userService;

    public GetUsersQueryHandler(IUserService userService)
    {
        _userService = userService;
    }

    public async Task<Response<PaginatedList<UserDto>>> Handle(GetUsersQuery request, CancellationToken cancellationToken)
    {
        var userListRequest = new UserListRequest
        {
            Page = request.Page,
            PageSize = request.PageSize,
            Search = request.Search,
            Role = request.Role,
            EmailConfirmed = request.EmailConfirmed
        };

        return await _userService.GetUsersAsync(userListRequest);
    }
}
