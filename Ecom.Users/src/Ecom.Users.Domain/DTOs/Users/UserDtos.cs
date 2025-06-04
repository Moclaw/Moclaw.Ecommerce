using System.ComponentModel.DataAnnotations;

namespace Ecom.Users.Domain.DTOs.Users;

public record UserDto
{
    public Guid Id { get; init; }
    public string Email { get; init; } = string.Empty;
    public string Username { get; init; } = string.Empty;
    public string FirstName { get; init; } = string.Empty;
    public string LastName { get; init; } = string.Empty;
    public string? PhoneNumber { get; init; }
    public bool EmailConfirmed { get; init; }
    public bool PhoneNumberConfirmed { get; init; }
    public bool TwoFactorEnabled { get; init; }
    public DateTimeOffset CreatedAt { get; init; }
    public DateTimeOffset? UpdatedAt { get; init; }
    public string Provider { get; init; } = string.Empty;
}

public record EditUserDto
{
    [StringLength(100)]
    public string? FirstName { get; init; }

    [StringLength(100)]
    public string? LastName { get; init; }

    [StringLength(100)]
    public string? Username { get; init; }

    [Phone]
    public string? PhoneNumber { get; init; }
}

public record UpdatePasswordDto
{
    [Required]
    public string CurrentPassword { get; init; } = string.Empty;

    [Required]
    [StringLength(100, MinimumLength = 6)]
    public string NewPassword { get; init; } = string.Empty;

    [Required]
    [Compare("NewPassword")]
    public string ConfirmNewPassword { get; init; } = string.Empty;
}

public record RoleDto
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string NormalizedName { get; init; } = string.Empty;
    public string? Description { get; init; }
}

public record PermissionDto
{
    public Guid Id { get; init; }
    public string Module { get; init; } = string.Empty;
    public string Action { get; init; } = string.Empty;
    public string? Resource { get; init; }
    public string? Description { get; init; }
    
    public string FullPermission => $"{Module}.{Action}{(!string.IsNullOrEmpty(Resource) ? $".{Resource}" : string.Empty)}";
}

public record PaginatedResponseDto<T>
{
    public IEnumerable<T> Items { get; init; } = [];
    public int PageNumber { get; init; }
    public int PageSize { get; init; }
    public int TotalCount { get; init; }
    public int TotalPages { get; init; }
    public bool HasPreviousPage => PageNumber > 1;
    public bool HasNextPage => PageNumber < TotalPages;
}