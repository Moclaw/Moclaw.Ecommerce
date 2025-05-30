namespace Ecom.Users.API.Filters;

/// <summary>
/// Authorization attribute that checks for specific permissions
/// Can be used with Refit for permission-based authorization
/// </summary>
[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = true)]
public class RequirePermissionAttribute : Attribute, IAuthorizationFilter
{
    private readonly string[] _permissions;
    private readonly bool _requireAll;

    /// <summary>
    /// Requires one or more permissions
    /// </summary>
    /// <param name="permissions">Required permissions</param>
    /// <param name="requireAll">If true, user must have all permissions. If false, user needs at least one permission.</param>
    public RequirePermissionAttribute(params string[] permissions)
    {
        _permissions = permissions ?? throw new ArgumentNullException(nameof(permissions));
        _requireAll = false;
    }

    /// <summary>
    /// Requires one or more permissions with option to require all
    /// </summary>
    /// <param name="requireAll">If true, user must have all permissions. If false, user needs at least one permission.</param>
    /// <param name="permissions">Required permissions</param>
    public RequirePermissionAttribute(bool requireAll, params string[] permissions)
    {
        _permissions = permissions ?? throw new ArgumentNullException(nameof(permissions));
        _requireAll = requireAll;
    }

    public void OnAuthorization(AuthorizationFilterContext context)
    {
        var user = context.HttpContext.User;

        if (!user.Identity?.IsAuthenticated ?? true)
        {
            context.Result = new UnauthorizedResult();
            return;
        }

        if (_permissions.Length == 0)
        {
            return; // No permissions required
        }

        var userPermissions = user.FindAll(AuthConstants.ClaimTypes.Permission)
                                 .Select(c => c.Value)
                                 .ToList();

        bool hasPermission;

        if (_requireAll)
        {
            // User must have ALL required permissions
            hasPermission = _permissions.All(permission => userPermissions.Contains(permission));
        }
        else
        {
            // User must have AT LEAST ONE of the required permissions
            hasPermission = _permissions.Any(permission => userPermissions.Contains(permission));
        }

        if (!hasPermission)
        {
            context.Result = new ForbidResult();
        }
    }
}

/// <summary>
/// Authorization attribute that checks for specific roles
/// </summary>
[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = true)]
public class RequireRoleAttribute : Attribute, IAuthorizationFilter
{
    private readonly string[] _roles;
    private readonly bool _requireAll;

    public RequireRoleAttribute(params string[] roles)
    {
        _roles = roles ?? throw new ArgumentNullException(nameof(roles));
        _requireAll = false;
    }

    public RequireRoleAttribute(bool requireAll, params string[] roles)
    {
        _roles = roles ?? throw new ArgumentNullException(nameof(roles));
        _requireAll = requireAll;
    }

    public void OnAuthorization(AuthorizationFilterContext context)
    {
        var user = context.HttpContext.User;

        if (!user.Identity?.IsAuthenticated ?? true)
        {
            context.Result = new UnauthorizedResult();
            return;
        }

        if (_roles.Length == 0)
        {
            return; // No roles required
        }

        bool hasRole;

        if (_requireAll)
        {
            // User must have ALL required roles
            hasRole = _roles.All(role => user.IsInRole(role));
        }
        else
        {
            // User must have AT LEAST ONE of the required roles
            hasRole = _roles.Any(role => user.IsInRole(role));
        }

        if (!hasRole)
        {
            context.Result = new ForbidResult();
        }
    }
}

/// <summary>
/// Combined authorization attribute that can check both roles and permissions
/// Useful for complex authorization scenarios
/// </summary>
[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]
public class RequireAuthorizationAttribute : Attribute, IAuthorizationFilter
{
    private readonly string[]? _roles;
    private readonly string[]? _permissions;
    private readonly bool _requireAllRoles;
    private readonly bool _requireAllPermissions;
    private readonly AuthorizationOperator _operator;

    public enum AuthorizationOperator
    {
        And, // User must satisfy both role AND permission requirements
        Or   // User must satisfy either role OR permission requirements
    }

    public RequireAuthorizationAttribute(
        string[]? roles = null,
        string[]? permissions = null,
        bool requireAllRoles = false,
        bool requireAllPermissions = false,
        AuthorizationOperator @operator = AuthorizationOperator.And)
    {
        _roles = roles;
        _permissions = permissions;
        _requireAllRoles = requireAllRoles;
        _requireAllPermissions = requireAllPermissions;
        _operator = @operator;
    }

    public void OnAuthorization(AuthorizationFilterContext context)
    {
        var user = context.HttpContext.User;

        if (!user.Identity?.IsAuthenticated ?? true)
        {
            context.Result = new UnauthorizedResult();
            return;
        }

        bool roleCheck = true;
        bool permissionCheck = true;

        // Check roles if specified
        if (_roles?.Length > 0)
        {
            if (_requireAllRoles)
            {
                roleCheck = _roles.All(role => user.IsInRole(role));
            }
            else
            {
                roleCheck = _roles.Any(role => user.IsInRole(role));
            }
        }

        // Check permissions if specified
        if (_permissions?.Length > 0)
        {
            var userPermissions = user.FindAll(AuthConstants.ClaimTypes.Permission)
                                     .Select(c => c.Value)
                                     .ToList();

            if (_requireAllPermissions)
            {
                permissionCheck = _permissions.All(permission => userPermissions.Contains(permission));
            }
            else
            {
                permissionCheck = _permissions.Any(permission => userPermissions.Contains(permission));
            }
        }

        // Combine results based on operator
        bool authorized = _operator switch
        {
            AuthorizationOperator.And => roleCheck && permissionCheck,
            AuthorizationOperator.Or => roleCheck || permissionCheck,
            _ => false
        };

        if (!authorized)
        {
            context.Result = new ForbidResult();
        }
    }
}
