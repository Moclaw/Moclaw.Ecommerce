namespace Ecom.Users.Domain.Entities;

public class User : BaseEntity, IEntity<Guid>
{
    public Guid Id { get; set; }
    
    public string Email { get; set; }
    
    public string? UserName { get; set; }
    
    public string? FirstName { get; set; }
    
    public string? LastName { get; set; }
    
    public string? PhoneNumber { get; set; }
    
    public string? PasswordHash { get; set; }
    
    public bool EmailConfirmed { get; set; } = false;
    
    public bool PhoneNumberConfirmed { get; set; } = false;
    
    public bool TwoFactorEnabled { get; set; } = false;
    
    public bool LockoutEnabled { get; set; } = true;
    
    public DateTimeOffset? LockoutEnd { get; set; }
    
    public int AccessFailedCount { get; set; } = 0;
    
    public string? SecurityStamp { get; set; }
    
    public string? ConcurrencyStamp { get; set; }
    
    public string? Provider { get; set; } // google, facebook, local
    
    public string? ProviderId { get; set; }

    // Navigation properties
    public ICollection<UserRole> UserRoles { get; set; } = [];
    
    public ICollection<RefreshToken> RefreshTokens { get; set; } = [];
}