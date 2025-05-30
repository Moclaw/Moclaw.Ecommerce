namespace Ecom.Users.Domain.Entities;

public class Role : BaseEntity, IEntity<Guid>
{
    public Guid Id { get; set; }
    
    public string Name { get; set; } = string.Empty;
    
    public string NormalizedName { get; set; } = string.Empty;
    
    public string? Description { get; set; }
    
    public bool IsSystemRole { get; set; }
    
    // Navigation Properties
    public virtual ICollection<UserRole> UserRoles { get; set; } = [];
    public virtual ICollection<RolePermission> RolePermissions { get; set; } = [];
}
