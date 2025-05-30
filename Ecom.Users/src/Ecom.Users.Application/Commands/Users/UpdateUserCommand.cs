namespace Ecom.Users.Application.Commands.Users;

public record UpdateUserCommand : IRequest<Response<UserDto>>
{
    public Guid UserId { get; init; }
    public string FirstName { get; init; } = string.Empty;
    public string LastName { get; init; } = string.Empty;
    public string? PhoneNumber { get; init; }
    public string? ProfileImageUrl { get; init; }
}

public class UpdateUserCommandHandler : IRequestHandler<UpdateUserCommand, Response<UserDto>>
{
    private readonly IUserService _userService;

    public UpdateUserCommandHandler(IUserService userService)
    {
        _userService = userService;
    }

    public async Task<Response<UserDto>> Handle(UpdateUserCommand request, CancellationToken cancellationToken)
    {
        var updateRequest = new UpdateUserRequest
        {
            FirstName = request.FirstName,
            LastName = request.LastName,
            PhoneNumber = request.PhoneNumber,
            ProfileImageUrl = request.ProfileImageUrl
        };

        return await _userService.UpdateUserAsync(request.UserId, updateRequest);
    }
}
