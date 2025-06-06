using Refit;

namespace Ecom.Users.Domain.Interfaces.External;

public interface IPermissionChecker
{
    [Get("/api/permissions/check")]
    Task<Response<bool>> CheckPermissionAsync(
        [Query] Guid userId, 
        [Query] string module, 
        [Query] string action, 
        [Query] string? resource = null);
}
