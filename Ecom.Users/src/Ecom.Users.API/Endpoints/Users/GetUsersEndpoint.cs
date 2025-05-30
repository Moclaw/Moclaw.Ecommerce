namespace Ecom.Users.API.Endpoints.Users;

public record GetUsersRequest : IQueryCollectionRequest<GetUsersResponse>
{
    public string? Search { get; set; }
    public int PageIndex { get; set; } = 0;
    public int PageSize { get; set; } = 10;
    public string OrderBy { get; set; } = "CreatedAt";
    public bool IsAscending { get; set; } = false;
}

public record GetUsersResponse
{
    public Guid Id { get; init; }
    public string Email { get; init; } = string.Empty;
    public string UserName { get; init; } = string.Empty;
    public string FirstName { get; init; } = string.Empty;
    public string LastName { get; init; } = string.Empty;
    public bool EmailConfirmed { get; init; }
    public DateTimeOffset CreatedAt { get; init; }
    public DateTimeOffset? LastLoginAt { get; init; }
}

[Summary("Get users")]
[Tags("Users")]
public class GetUsersEndpoint : CollectionEndpointBase<GetUsersRequest, GetUsersResponse>
{
    public override void Configure()
    {
        Get("/users");
        Policies("CanViewUsers");
        Version(1);
        Summary(s =>
        {
            s.Summary = "Get paginated list of users";
            s.Description = "Retrieves a paginated list of users (Admin/Employee only)";
        });
    }

    public override async Task<ResponseCollection<GetUsersResponse>> ExecuteAsync(GetUsersRequest request, CancellationToken ct)
    {
        var query = new GetUsersQuery
        {
            Page = request.PageIndex + 1, // Convert from 0-based to 1-based
            PageSize = request.PageSize,
            SearchTerm = request.Search
        };

        var result = await new GetUsersQueryHandler(
            Resolve<IQueryRepository<User, Guid>>()
        ).Handle(query, ct);

        return new ResponseCollection<GetUsersResponse>(
            IsSuccess: result.IsSuccess,
            result.StatusCode,
            result.Message,
            Data: result.Data?.Items?.Select(u => new GetUsersResponse
            {
                Id = u.Id,
                Email = u.Email,
                UserName = u.UserName,
                FirstName = u.FirstName,
                LastName = u.LastName,
                EmailConfirmed = u.EmailConfirmed,
                CreatedAt = u.CreatedAt,
                LastLoginAt = u.LastLoginAt
            }).ToList() ?? [],
            TotalCount: result.Data?.TotalCount ?? 0,
            PageIndex: request.PageIndex,
            PageSize: request.PageSize
        );
    }
}
