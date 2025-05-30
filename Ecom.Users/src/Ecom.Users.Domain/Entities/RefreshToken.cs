namespace Ecom.Users.Domain.Entities;

public class RefreshToken : BaseEntity, IEntity<Guid>
{
    public Guid Id { get; set; }
    
    public Guid UserId { get; set; }
    
    public string Token { get; set; } = string.Empty;
    
    public DateTimeOffset ExpiresAt { get; set; }
    
    public bool IsRevoked { get; set; }
    
    public DateTimeOffset? RevokedAt { get; set; }
    
    public string? RevokedByIp { get; set; }
    
    public string? ReplacedByToken { get; set; }
    
    public string? ReasonRevoked { get; set; }
    
    public string CreatedByIp { get; set; } = string.Empty;
    
    public bool IsExpired => DateTimeOffset.UtcNow >= ExpiresAt;
    
    public bool IsActive => !IsRevoked && !IsExpired;
    
    // Navigation Properties
    public virtual User User { get; set; } = null!;
}
