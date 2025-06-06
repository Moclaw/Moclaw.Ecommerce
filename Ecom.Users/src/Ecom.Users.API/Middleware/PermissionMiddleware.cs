using Ecom.Users.Domain.Interfaces;
using Microsoft.AspNetCore.Authorization;

namespace Ecom.Users.API.Middleware;

public class PermissionMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(HttpContext context, IUserService userService)
    {
        // Only check permission for endpoints that require authorization
        var endpoint = context.GetEndpoint();
        if (endpoint?.Metadata?.GetMetadata<IAuthorizeData>() == null)
        {
            await next(context);
            return;
        }

        // Get user ID from claims
        var userIdClaim = context.User.FindFirst("sub")?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            return;
        }

        // Get required permission from endpoint metadata
        var requiredPermission = endpoint.Metadata.GetMetadata<RequiresPermissionAttribute>();
        if (requiredPermission == null)
        {
            // No specific permission required, continue
            await next(context);
            return;
        }

        // Check if user has required permission
        bool hasPermission = await userService.HasPermissionAsync(
            userId,
            requiredPermission.Module,
            requiredPermission.Action,
            requiredPermission.Resource);

        if (!hasPermission)
        {
            context.Response.StatusCode = StatusCodes.Status403Forbidden;
            return;
        }

        await next(context);
    }
}

// Extension method to add the middleware
public static class PermissionMiddlewareExtensions
{
    public static IApplicationBuilder UsePermissionMiddleware(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<PermissionMiddleware>();
    }
}

// Attribute to specify required permission
[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = true)]
public class RequiresPermissionAttribute(string module, string action, string? resource = null) : Attribute
{
    public string Module { get; } = module;
    public string Action { get; } = action;
    public string? Resource { get; } = resource;
}
