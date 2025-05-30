namespace Ecom.Users.Domain.Constants;

public static class Roles
{
    public const string Admin = "Admin";
    public const string Employee = "Employee";
    public const string User = "User";
}

public static class Permissions
{
    // User Management
    public const string UserRead = "user:read";
    public const string UserWrite = "user:write";
    public const string UserDelete = "user:delete";
    public const string UserManage = "user:manage";
    
    // Role Management
    public const string RoleRead = "role:read";
    public const string RoleWrite = "role:write";
    public const string RoleDelete = "role:delete";
    public const string RoleManage = "role:manage";
    
    // Permission Management
    public const string PermissionRead = "permission:read";
    public const string PermissionWrite = "permission:write";
    public const string PermissionDelete = "permission:delete";
    public const string PermissionManage = "permission:manage";
    
    // Products (for cross-module usage)
    public const string ProductRead = "product:read";
    public const string ProductWrite = "product:write";
    public const string ProductDelete = "product:delete";
    public const string ProductManage = "product:manage";
}

public static class ClaimTypes
{
    public const string UserId = "uid";
    public const string Email = "email";
    public const string Username = "username";
    public const string FirstName = "given_name";
    public const string LastName = "family_name";
    public const string Role = "role";
    public const string Permission = "permission";
}
