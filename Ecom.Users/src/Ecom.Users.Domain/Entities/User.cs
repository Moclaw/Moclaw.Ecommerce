namespace Ecom.Users.Domain.Entities;

public class User : BaseEntity, IEntity<Guid>
{
    public Guid Id { get; set; }
    
    public string Email { get; set; } = string.Empty;
    
    public string UserName { get; set; } = string.Empty;
    
    public string FirstName { get; set; } = string.Empty;
    
    public string LastName { get; set; } = string.Empty;
    
    public string? PhoneNumber { get; set; }
    
    public string PasswordHash { get; set; } = string.Empty;
    
    public bool EmailConfirmed { get; set; }
    
    public bool PhoneNumberConfirmed { get; set; }
    
    public bool TwoFactorEnabled { get; set; }
    
    public DateTimeOffset? LockoutEnd { get; set; }
    
    public bool LockoutEnabled { get; set; }
    
    public int AccessFailedCount { get; set; }
    
    public string? ProfileImageUrl { get; set; }
    
    public DateTimeOffset? LastLoginAt { get; set; }
    
    // Social Login
    public string? GoogleId { get; set; }
    public string? FacebookId { get; set; }
    
    // Navigation Properties
    public virtual ICollection<UserRole> UserRoles { get; set; } = [];
    public virtual ICollection<RefreshToken> RefreshTokens { get; set; } = [];
}
