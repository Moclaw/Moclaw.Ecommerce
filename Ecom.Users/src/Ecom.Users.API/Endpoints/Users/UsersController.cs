namespace Ecom.Users.API.Endpoints.Users;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class UsersController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<UsersController> _logger;

    public UsersController(IMediator mediator, ILogger<UsersController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    /// Get paginated list of users (Admin/Employee only)
    /// </summary>
    [HttpGet]
    [Authorize(Policy = "CanViewUsers")]
    public async Task<ActionResult<PagedResult<UserDto>>> GetUsers(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? searchTerm = null)
    {
        var query = new GetUsersQuery
        {
            Page = page,
            PageSize = pageSize,
            SearchTerm = searchTerm
        };

        var result = await _mediator.Send(query);
        return Ok(result);
    }

    /// <summary>
    /// Get user by ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<UserDto>> GetUser(int id)
    {
        var currentUserId = GetCurrentUserId();
        
        // Users can only view their own profile unless they have admin/employee permissions
        if (currentUserId != id && !HasPermission(AuthConstants.Permissions.Users.View))
        {
            return Forbid("You can only view your own profile");
        }

        var query = new GetUserByIdQuery { UserId = id };
        var result = await _mediator.Send(query);

        if (result == null)
        {
            return NotFound($"User with ID {id} not found");
        }

        return Ok(result);
    }

    /// <summary>
    /// Get current user's profile
    /// </summary>
    [HttpGet("me")]
    public async Task<ActionResult<UserDto>> GetCurrentUser()
    {
        var currentUserId = GetCurrentUserId();
        var query = new GetUserByIdQuery { UserId = currentUserId };
        var result = await _mediator.Send(query);

        if (result == null)
        {
            return NotFound("Current user not found");
        }

        return Ok(result);
    }

    /// <summary>
    /// Update user information
    /// </summary>
    [HttpPut("{id}")]
    public async Task<ActionResult<UserDto>> UpdateUser(int id, [FromBody] UpdateUserRequest request)
    {
        var currentUserId = GetCurrentUserId();
        
        // Users can only update their own profile unless they have admin permissions
        if (currentUserId != id && !HasPermission(AuthConstants.Permissions.Users.Update))
        {
            return Forbid("You can only update your own profile");
        }

        var command = new UpdateUserCommand
        {
            UserId = id,
            Email = request.Email,
            Username = request.Username,
            FirstName = request.FirstName,
            LastName = request.LastName,
            PhoneNumber = request.PhoneNumber
        };

        var result = await _mediator.Send(command);
        return Ok(result);
    }

    /// <summary>
    /// Update current user's profile
    /// </summary>
    [HttpPut("me")]
    public async Task<ActionResult<UserDto>> UpdateCurrentUser([FromBody] UpdateUserRequest request)
    {
        var currentUserId = GetCurrentUserId();
        
        var command = new UpdateUserCommand
        {
            UserId = currentUserId,
            Email = request.Email,
            Username = request.Username,
            FirstName = request.FirstName,
            LastName = request.LastName,
            PhoneNumber = request.PhoneNumber
        };

        var result = await _mediator.Send(command);
        return Ok(result);
    }

    /// <summary>
    /// Change user password
    /// </summary>
    [HttpPost("{id}/change-password")]
    public async Task<ActionResult> ChangePassword(int id, [FromBody] ChangePasswordRequest request)
    {
        var currentUserId = GetCurrentUserId();
        
        // Users can only change their own password unless they have admin permissions
        if (currentUserId != id && !HasPermission(AuthConstants.Permissions.Users.Update))
        {
            return Forbid("You can only change your own password");
        }

        var command = new ChangePasswordCommand
        {
            UserId = id,
            CurrentPassword = request.CurrentPassword,
            NewPassword = request.NewPassword
        };

        await _mediator.Send(command);
        return Ok(new { message = "Password changed successfully" });
    }

    /// <summary>
    /// Change current user's password
    /// </summary>
    [HttpPost("me/change-password")]
    public async Task<ActionResult> ChangeCurrentUserPassword([FromBody] ChangePasswordRequest request)
    {
        var currentUserId = GetCurrentUserId();
        
        var command = new ChangePasswordCommand
        {
            UserId = currentUserId,
            CurrentPassword = request.CurrentPassword,
            NewPassword = request.NewPassword
        };

        await _mediator.Send(command);
        return Ok(new { message = "Password changed successfully" });
    }

    /// <summary>
    /// Get user permissions
    /// </summary>
    [HttpGet("{id}/permissions")]
    public async Task<ActionResult<List<string>>> GetUserPermissions(int id)
    {
        var currentUserId = GetCurrentUserId();
        
        // Users can only view their own permissions unless they have admin permissions
        if (currentUserId != id && !HasPermission(AuthConstants.Permissions.Users.View))
        {
            return Forbid("You can only view your own permissions");
        }

        var query = new GetUserPermissionsQuery { UserId = id };
        var result = await _mediator.Send(query);
        return Ok(result);
    }

    /// <summary>
    /// Get current user's permissions
    /// </summary>
    [HttpGet("me/permissions")]
    public async Task<ActionResult<List<string>>> GetCurrentUserPermissions()
    {
        var currentUserId = GetCurrentUserId();
        var query = new GetUserPermissionsQuery { UserId = currentUserId };
        var result = await _mediator.Send(query);
        return Ok(result);
    }

    private int GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(AuthConstants.ClaimTypes.UserId)?.Value;
        return int.TryParse(userIdClaim, out var userId) ? userId : 0;
    }

    private bool HasPermission(string permission)
    {
        return User.HasClaim(AuthConstants.ClaimTypes.Permission, permission);
    }

    private bool HasRole(string role)
    {
        return User.IsInRole(role);
    }
}

// Request models for the API endpoints
public class UpdateUserRequest
{
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;
    
    public string? Username { get; set; }
    
    [Required]
    public string FirstName { get; set; } = string.Empty;
    
    [Required]
    public string LastName { get; set; } = string.Empty;
    
    public string? PhoneNumber { get; set; }
}

public class ChangePasswordRequest
{
    [Required]
    public string CurrentPassword { get; set; } = string.Empty;
    
    [Required]
    [MinLength(6)]
    public string NewPassword { get; set; } = string.Empty;
}
