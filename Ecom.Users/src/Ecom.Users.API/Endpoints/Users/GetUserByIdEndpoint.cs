namespace Ecom.Users.API.Endpoints.Users;

public record GetUserByIdRequest
{
    [FromRoute]
    public Guid Id { get; init; }
}

public record GetUserByIdResponse
{
    public Guid Id { get; init; }
    public string Email { get; init; } = string.Empty;
    public string UserName { get; init; } = string.Empty;
    public string FirstName { get; init; } = string.Empty;
    public string LastName { get; init; } = string.Empty;
    public string? PhoneNumber { get; init; }
    public bool EmailConfirmed { get; init; }
    public bool PhoneNumberConfirmed { get; init; }
    public bool TwoFactorEnabled { get; init; }
    public bool LockoutEnabled { get; init; }
    public DateTimeOffset CreatedAt { get; init; }
    public DateTimeOffset? LastLoginAt { get; init; }
    public List<string> Roles { get; init; } = [];
}

[Summary("Get user by ID")]
[Tags("Users")]
public class GetUserByIdEndpoint : EndpointBase<GetUserByIdRequest, Response<GetUserByIdResponse>>
{
    public override void Configure()
    {
        Get("/users/{id}");
        Policies("CanViewUsers");
        Version(1);
        Summary(s =>
        {
            s.Summary = "Get user by ID";
            s.Description = "Retrieves detailed information about a specific user";
        });
    }

    public override async Task<Response<GetUserByIdResponse>> ExecuteAsync(GetUserByIdRequest request, CancellationToken ct)
    {
        var query = new GetUserByIdQuery { Id = request.Id };

        var result = await new GetUserByIdQueryHandler(
            Resolve<IQueryRepository<User, Guid>>()
        ).Handle(query, ct);

        if (!result.IsSuccess || result.Data == null)
        {
            return new Response<GetUserByIdResponse>(
                IsSuccess: false,
                result.StatusCode,
                result.Message,
                Data: null
            );
        }

        var response = new GetUserByIdResponse
        {
            Id = result.Data.Id,
            Email = result.Data.Email,
            UserName = result.Data.UserName,
            FirstName = result.Data.FirstName,
            LastName = result.Data.LastName,
            PhoneNumber = result.Data.PhoneNumber,
            EmailConfirmed = result.Data.EmailConfirmed,
            PhoneNumberConfirmed = result.Data.PhoneNumberConfirmed,
            TwoFactorEnabled = result.Data.TwoFactorEnabled,
            LockoutEnabled = result.Data.LockoutEnabled,
            CreatedAt = result.Data.CreatedAt,
            LastLoginAt = result.Data.LastLoginAt,
            Roles = result.Data.Roles ?? []
        };

        return new Response<GetUserByIdResponse>(
            IsSuccess: true,
            200,
            "User retrieved successfully",
            Data: response
        );
    }
}
