namespace Ecom.Users.Domain.Entities;

public class UserRole : IEntity<Guid>
{
    public Guid Id { get; set; }
    
    public Guid UserId { get; set; }
    
    public Guid RoleId { get; set; }
    
    public DateTimeOffset AssignedAt { get; set; } = DateTimeOffset.UtcNow;
    
    public Guid AssignedBy { get; set; }
    
    // Navigation Properties
    public virtual User User { get; set; } = null!;
    public virtual Role Role { get; set; } = null!;
}
