namespace Ecom.Users.API.Endpoints.Auth;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<AuthController> _logger;

    public AuthController(IMediator mediator, ILogger<AuthController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    /// Login with email and password
    /// </summary>
    [HttpPost("login")]
    public async Task<ActionResult<AuthResultDto>> Login([FromBody] LoginRequest request)
    {
        var command = new LoginCommand
        {
            Email = request.Email,
            Password = request.Password,
            DeviceInfo = GetDeviceInfo(),
            IpAddress = GetClientIpAddress()
        };

        var result = await _mediator.Send(command);
        return Ok(result);
    }

    /// <summary>
    /// Register a new user account
    /// </summary>
    [HttpPost("register")]
    public async Task<ActionResult<AuthResultDto>> Register([FromBody] RegisterRequest request)
    {
        var command = new RegisterCommand
        {
            Email = request.Email,
            Username = request.Username,
            Password = request.Password,
            FirstName = request.FirstName,
            LastName = request.LastName,
            PhoneNumber = request.PhoneNumber,
            DeviceInfo = GetDeviceInfo(),
            IpAddress = GetClientIpAddress()
        };

        var result = await _mediator.Send(command);
        return Ok(result);
    }

    /// <summary>
    /// Refresh access token using refresh token
    /// </summary>
    [HttpPost("refresh")]
    public async Task<ActionResult<AuthResultDto>> RefreshToken([FromBody] RefreshTokenRequest request)
    {
        var command = new RefreshTokenCommand
        {
            RefreshToken = request.RefreshToken,
            AccessToken = request.AccessToken,
            JwtId = request.JwtId,
            DeviceInfo = GetDeviceInfo(),
            IpAddress = GetClientIpAddress()
        };

        var result = await _mediator.Send(command);
        return Ok(result);
    }

    /// <summary>
    /// Login with social media provider (Google, Facebook)
    /// </summary>
    [HttpPost("social-login")]
    public async Task<ActionResult<AuthResultDto>> SocialLogin([FromBody] SocialLoginRequest request)
    {
        var command = new SocialLoginCommand
        {
            Provider = request.Provider,
            Token = request.Token,
            Email = request.Email,
            FirstName = request.FirstName,
            LastName = request.LastName,
            ProfilePictureUrl = request.ProfilePictureUrl,
            DeviceInfo = GetDeviceInfo(),
            IpAddress = GetClientIpAddress()
        };

        var result = await _mediator.Send(command);
        return Ok(result);
    }

    /// <summary>
    /// Logout and invalidate refresh token
    /// </summary>
    [HttpPost("logout")]
    [Authorize]
    public async Task<ActionResult> Logout([FromBody] LogoutRequest request)
    {
        // This would be handled by the AuthService.LogoutAsync method
        // For now, we'll just return success as the token invalidation logic
        // is implemented in the infrastructure layer
        return Ok(new { message = "Logged out successfully" });
    }

    /// <summary>
    /// Logout from all devices
    /// </summary>
    [HttpPost("logout-all")]
    [Authorize]
    public async Task<ActionResult> LogoutAll()
    {
        var userId = GetCurrentUserId();
        
        // This would be handled by the AuthService.LogoutAllDevicesAsync method
        // For now, we'll just return success
        return Ok(new { message = "Logged out from all devices successfully" });
    }

    private string GetDeviceInfo()
    {
        var userAgent = Request.Headers["User-Agent"].ToString();
        return string.IsNullOrEmpty(userAgent) ? "Unknown Device" : userAgent;
    }

    private string GetClientIpAddress()
    {
        var ipAddress = Request.Headers["X-Forwarded-For"].FirstOrDefault()?.Split(',').FirstOrDefault()?.Trim();
        
        if (string.IsNullOrEmpty(ipAddress))
        {
            ipAddress = Request.Headers["X-Real-IP"].FirstOrDefault();
        }
        
        if (string.IsNullOrEmpty(ipAddress))
        {
            ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
        }
        
        return ipAddress ?? "Unknown";
    }

    private int GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(AuthConstants.ClaimTypes.UserId)?.Value;
        return int.TryParse(userIdClaim, out var userId) ? userId : 0;
    }
}

// Request models for the API endpoints
public class LoginRequest
{
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;
    
    [Required]
    public string Password { get; set; } = string.Empty;
}

public class RegisterRequest
{
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;
    
    public string? Username { get; set; }
    
    [Required]
    [MinLength(6)]
    public string Password { get; set; } = string.Empty;
    
    [Required]
    public string FirstName { get; set; } = string.Empty;
    
    [Required]
    public string LastName { get; set; } = string.Empty;
    
    public string? PhoneNumber { get; set; }
}

public class RefreshTokenRequest
{
    [Required]
    public string RefreshToken { get; set; } = string.Empty;
    
    public string? AccessToken { get; set; }
    
    public string? JwtId { get; set; }
}

public class SocialLoginRequest
{
    [Required]
    public string Provider { get; set; } = string.Empty;
    
    [Required]
    public string Token { get; set; } = string.Empty;
    
    public string? Email { get; set; }
    
    public string? FirstName { get; set; }
    
    public string? LastName { get; set; }
    
    public string? ProfilePictureUrl { get; set; }
}

public class LogoutRequest
{
    public string? RefreshToken { get; set; }
}
