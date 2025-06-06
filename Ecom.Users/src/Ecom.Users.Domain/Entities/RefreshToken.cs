namespace Ecom.Users.Domain.Entities;

public class RefreshToken : BaseEntity, IEntity<Guid>
{
    public Guid Id { get; set; }
    
    public string Token { get; set; } = null!;
    
    public DateTimeOffset ExpiryDate { get; set; }
    
    public bool IsUsed { get; set; } = false;
    
    public bool IsRevoked { get; set; } = false;
    
    public DateTimeOffset? RevokedAt { get; set; }
    
    public string? ReplacedByToken { get; set; }
    
    public Guid UserId { get; set; }
    
    public string JwtId { get; set; } = null!;
    
    public string? IpAddress { get; set; }
    
    public string? UserAgent { get; set; }

    // Navigation properties
    public User User { get; set; } = null!;
    
    // Helper properties
    public bool IsExpired => DateTimeOffset.UtcNow >= ExpiryDate;
    
    public bool IsActive => !IsRevoked && !IsExpired && !IsUsed;
}