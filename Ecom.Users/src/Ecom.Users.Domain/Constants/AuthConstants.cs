namespace Ecom.Users.Domain.Constants;

public static class AuthConstants
{
    public static class Roles
    {
        public const string Admin = "Admin";
        public const string Employee = "Employee";
        public const string User = "User";
    }

    public static class Modules
    {
        public const string Users = "Users";
        public const string Roles = "Roles";
        public const string Permissions = "Permissions";
    }

    public static class Actions
    {
        public const string Create = "Create";
        public const string Read = "Read";
        public const string Update = "Update";
        public const string Delete = "Delete";
        public const string List = "List";
    }

    public static class Permissions
    {
        // User Management
        public const string UsersRead = "Users.Read";
        public const string UsersCreate = "Users.Create";
        public const string UsersUpdate = "Users.Update";
        public const string UsersDelete = "Users.Delete";
        public const string UsersList = "Users.List";
        
        // Role Management
        public const string RolesRead = "Roles.Read";
        public const string RolesCreate = "Roles.Create";
        public const string RolesUpdate = "Roles.Update";
        public const string RolesDelete = "Roles.Delete";
        
        // Permission Management
        public const string PermissionsRead = "Permissions.Read";
        public const string PermissionsCreate = "Permissions.Create";
        public const string PermissionsUpdate = "Permissions.Update";
        public const string PermissionsDelete = "Permissions.Delete";
    }

    public static class Providers
    {
        public const string Local = "Local";
        public const string Google = "Google";
        public const string Facebook = "Facebook";
    }

    public static class ClaimTypes
    {
        public const string UserId = "user_id";
        public const string Email = "email";
        public const string Role = "role";
        public const string Permission = "permission";
        public const string Provider = "provider";
        public const string UserName = "username";
    }

    public static class TokenTypes
    {
        public const string AccessToken = "access_token";
        public const string RefreshToken = "refresh_token";
    }
}