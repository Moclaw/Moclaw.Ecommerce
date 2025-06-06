using Ecom.Users.Domain.Constants;
using System.ComponentModel.DataAnnotations;

namespace Ecom.Users.Domain.DTOs;

public record LoginDto
{
    [Required]
    [EmailAddress]
    public string Email { get; init; } = string.Empty;

    [Required]
    public string Password { get; init; } = string.Empty;

    public bool RememberMe { get; init; } = false;
}

public record RegisterDto
{
    [Required]
    [EmailAddress]
    public string Email { get; init; } = string.Empty;

    [Required]
    [StringLength(100, MinimumLength = 6)]
    public string Password { get; init; } = string.Empty;

    [Required]
    [Compare("Password")]
    public string ConfirmPassword { get; init; } = string.Empty;

    [Required]
    [StringLength(100)]
    public string FirstName { get; init; } = string.Empty;

    [Required]
    [StringLength(100)]
    public string LastName { get; init; } = string.Empty;

    [Phone]
    public string? PhoneNumber { get; init; }

    public string? UserName { get; init; }
}

public record AuthResponseDto
{
    public Guid UserId { get; init; }
    public string Email { get; init; } = string.Empty;
    public string UserName { get; init; } = string.Empty;
    public string FirstName { get; init; } = string.Empty;
    public string LastName { get; init; } = string.Empty;
    public string AccessToken { get; init; } = string.Empty;
    public string RefreshToken { get; init; } = string.Empty;
    public DateTimeOffset ExpiresAt { get; init; }
    public IEnumerable<string> Roles { get; init; } = Array.Empty<string>();
    public IEnumerable<string> Permissions { get; init; } = Array.Empty<string>();
}

public record RefreshTokenDto
{
    [Required]
    public string RefreshToken { get; init; } = string.Empty;
}

public record OAuthCallbackDto
{
    [Required]
    public string Provider { get; init; } = string.Empty;

    [Required]
    public string Code { get; init; } = string.Empty;

    public string? RedirectUri { get; init; }
}

public record ExternalAuthDto
{
    [Required]
    public string Provider { get; init; } = AuthConstants.Providers.Google;

    [Required]
    public string AccessToken { get; init; } = string.Empty;

    public string? Name { get; init; }

    [Required]
    [EmailAddress]
    public string Email { get; init; } = string.Empty;

    public string? ProviderId { get; init; }
}
