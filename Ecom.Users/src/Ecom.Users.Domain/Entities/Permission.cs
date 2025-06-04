namespace Ecom.Users.Domain.Entities;

public class Permission : BaseEntity, IEntity<Guid>
{
    public Guid Id { get; set; }
    
    public string Module { get; set; } = null!; // e.g., "Users", "Products", "Orders"
    
    public string Action { get; set; } = null!; // e.g., "Create", "Read", "Update", "Delete"
    
    public string? Resource { get; set; } // e.g., "User", "Product", "Order"
    
    public string? Description { get; set; }

    // Navigation properties
    public ICollection<RolePermission> RolePermissions { get; set; } = [];
    
    // Computed property for full permission string
    public string FullPermission => $"{Module}.{Action}{(!string.IsNullOrEmpty(Resource) ? $".{Resource}" : string.Empty)}";
}