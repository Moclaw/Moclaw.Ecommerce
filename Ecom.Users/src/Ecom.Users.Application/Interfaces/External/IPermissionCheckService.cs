using Refit;

namespace Ecom.Users.Application.Interfaces.External;

/// <summary>
/// Refit interface for external permission checking
/// Can be used to validate permissions against external services
/// </summary>
public interface IPermissionCheckService
{
    /// <summary>
    /// Check if user has specific permission
    /// </summary>
    [Get("/api/permissions/check")]
    Task<bool> HasPermissionAsync(
        [Query] int userId, 
        [Query] string permission, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Check if user has all specified permissions
    /// </summary>
    [Post("/api/permissions/check-multiple")]
    Task<Dictionary<string, bool>> HasPermissionsAsync(
        [Body] CheckMultiplePermissionsRequest request, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get all permissions for a user
    /// </summary>
    [Get("/api/permissions/user/{userId}")]
    Task<List<string>> GetUserPermissionsAsync(
        int userId, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Validate permission against external authorization service
    /// </summary>
    [Post("/api/permissions/validate")]
    Task<PermissionValidationResponse> ValidatePermissionAsync(
        [Body] PermissionValidationRequest request, 
        CancellationToken cancellationToken = default);
}

public class CheckMultiplePermissionsRequest
{
    public int UserId { get; set; }
    public List<string> Permissions { get; set; } = new();
}

public class PermissionValidationRequest
{
    public int UserId { get; set; }
    public string Permission { get; set; } = string.Empty;
    public string? Resource { get; set; }
    public string? Action { get; set; }
    public Dictionary<string, object>? Context { get; set; }
}

public class PermissionValidationResponse
{
    public bool IsAllowed { get; set; }
    public string? Reason { get; set; }
    public Dictionary<string, object>? Metadata { get; set; }
}
