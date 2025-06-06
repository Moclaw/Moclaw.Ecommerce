namespace Ecom.Users.Domain.Entities;

public class UserRole : BaseEntity, IEntity<Guid>
{
    public Guid Id { get; set; }
    
    public Guid UserId { get; set; }
    
    public Guid RoleId { get; set; }

    // Navigation properties
    public User User { get; set; } = null!;
    
    public Role Role { get; set; } = null!;
}