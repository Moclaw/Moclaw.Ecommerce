using Ecom.Users.Application.DTOs.Users;

namespace Ecom.Users.Application.Interfaces;

public interface IUserService
{
    Task<Response<PaginatedList<UserDto>>> GetUsersAsync(UserListRequest request);
    Task<Response<UserDto>> GetUserByIdAsync(Guid userId);
    Task<Response<UserDto>> UpdateUserAsync(Guid userId, UpdateUserRequest request);
    Task<Response<bool>> ChangePasswordAsync(Guid userId, ChangePasswordRequest request);
    Task<Response<List<string>>> GetUserPermissionsAsync(Guid userId);
}
