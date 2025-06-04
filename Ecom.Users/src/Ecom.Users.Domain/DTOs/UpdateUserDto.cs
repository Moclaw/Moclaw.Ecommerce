using System.ComponentModel.DataAnnotations;

namespace Ecom.Users.Domain.DTOs;

public class UpdateUserDto
{
    [EmailAddress]
    public string? Email { get; set; }

    [StringLength(50)]
    public string? UserName { get; set; }

    [StringLength(100)]
    public string? FirstName { get; set; }

    [StringLength(100)]
    public string? LastName { get; set; }

    [Phone]
    public string? PhoneNumber { get; set; }

    [StringLength(100, MinimumLength = 6)]
    public string? CurrentPassword { get; set; }

    [StringLength(100, MinimumLength = 6)]
    public string? NewPassword { get; set; }
}
