namespace Ecom.Users.Domain.Entities;

public class Permission : BaseEntity, IEntity<Guid>
{
    public Guid Id { get; set; }
    
    public string Name { get; set; } = string.Empty;
    
    public string NormalizedName { get; set; } = string.Empty;
    
    public string? Description { get; set; }
    
    public string Module { get; set; } = string.Empty;
    
    public string Action { get; set; } = string.Empty;
    
    public string? Resource { get; set; }
    
    // Navigation Properties
    public virtual ICollection<RolePermission> RolePermissions { get; set; } = [];
}
