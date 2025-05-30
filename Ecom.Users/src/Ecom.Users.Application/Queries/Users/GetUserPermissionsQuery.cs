namespace Ecom.Users.Application.Queries.Users;

public record GetUserPermissionsQuery : IRequest<Response<List<string>>>
{
    public Guid UserId { get; init; }
}

public class GetUserPermissionsQueryHandler : IRequestHandler<GetUserPermissionsQuery, Response<List<string>>>
{
    private readonly IUserService _userService;

    public GetUserPermissionsQueryHandler(IUserService userService)
    {
        _userService = userService;
    }

    public async Task<Response<List<string>>> Handle(GetUserPermissionsQuery request, CancellationToken cancellationToken)
    {
        return await _userService.GetUserPermissionsAsync(request.UserId);
    }
}
