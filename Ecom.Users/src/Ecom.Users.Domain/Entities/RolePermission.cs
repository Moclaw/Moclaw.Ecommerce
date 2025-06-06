namespace Ecom.Users.Domain.Entities;

public class RolePermission : BaseEntity, IEntity<Guid>
{
    public Guid Id { get; set; }
    
    public Guid RoleId { get; set; }
    
    public Guid PermissionId { get; set; }

    // Navigation properties
    public Role Role { get; set; } = null!;
    
    public Permission Permission { get; set; } = null!;
}