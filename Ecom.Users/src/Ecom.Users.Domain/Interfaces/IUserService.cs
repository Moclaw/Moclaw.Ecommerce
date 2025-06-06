using Ecom.Users.Domain.DTOs;
using Ecom.Users.Domain.DTOs.Users;

namespace Ecom.Users.Domain.Interfaces;

public interface IUserService
{
    Task<Response<UserDto>> GetUserByIdAsync(Guid userId);
    Task<Response<UserDto>> GetUserByEmailAsync(string email);
    Task<ResponseCollection<UserDto>> GetUsersAsync(
        int pageNumber = 1,
        int pageSize = 10,
        string? searchTerm = null);
    Task<Response<UserDto>> UpdateUserAsync(Guid userId, UpdateUserDto updateUserDto);
    Task<Response<bool>> UpdatePasswordAsync(Guid userId, UpdatePasswordDto updatePasswordDto);
    Task<Response<IEnumerable<RoleDto>>> GetUserRolesAsync(Guid userId);
    Task<Response<IEnumerable<PermissionDto>>> GetUserPermissionsAsync(Guid userId);
    Task<Response<bool>> AssignRoleToUserAsync(Guid userId, Guid roleId);
    Task<Response<bool>> RemoveRoleFromUserAsync(Guid userId, Guid roleId);
    Task<Response<bool>> DeleteUserAsync(Guid userId);
    Task<bool> HasPermissionAsync(Guid userId, string module, string action, string? resource = null);
}
