namespace Ecom.Users.Domain.Entities;

public class RolePermission : IEntity<Guid>
{
    public Guid Id { get; set; }
    
    public Guid RoleId { get; set; }
    
    public Guid PermissionId { get; set; }
    
    public DateTimeOffset GrantedAt { get; set; } = DateTimeOffset.UtcNow;
    
    public Guid GrantedBy { get; set; }
    
    // Navigation Properties
    public virtual Role Role { get; set; } = null!;
    public virtual Permission Permission { get; set; } = null!;
}
