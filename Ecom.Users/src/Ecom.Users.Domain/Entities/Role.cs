namespace Ecom.Users.Domain.Entities;

public class Role : BaseEntity, IEntity<Guid>
{
    public Guid Id { get; set; }
    
    public string Name { get; set; } = null!;
    
    public string? Description { get; set; }
    
    public string? NormalizedName { get; set; }
    
    public string? ConcurrencyStamp { get; set; }

    // Navigation properties
    public ICollection<UserRole> UserRoles { get; set; } = [];
    
    public ICollection<RolePermission> RolePermissions { get; set; } = [];
}