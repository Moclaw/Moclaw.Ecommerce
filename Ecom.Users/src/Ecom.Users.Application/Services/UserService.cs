using Ecom.Users.Domain.DTOs;
using Ecom.Users.Domain.Entities;
using Ecom.Users.Domain.Interfaces;
using Ecom.Users.Domain.Constants;
using EfCore.IRepositories;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using Services.Autofac.Attributes;
using Shared.Utils;
using Ecom.Users.Domain.DTOs.Users;

namespace Ecom.Users.Application.Services;
[TransientService]
public class UserService(
    [FromKeyedServices(ServiceKeys.CommandRepository)] ICommandRepository commandRepository,
    [FromKeyedServices(ServiceKeys.QueryRepository)] IQueryRepository<User, Guid> userRepository,
    [FromKeyedServices(ServiceKeys.QueryRepository)] IQueryRepository<Role, Guid> roleRepository,
    [FromKeyedServices(ServiceKeys.QueryRepository)] IQueryRepository<UserRole, Guid> userRoleRepository,
    [FromKeyedServices(ServiceKeys.QueryRepository)] IQueryRepository<RolePermission, Guid> rolePermissionRepository,
    IPasswordHasher passwordHasher,
    ILogger<UserService> logger) : IUserService
{
    public async Task<Response<UserDto>> GetUserByIdAsync(Guid userId)
    {
        try
        {
            var user = await userRepository
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null)
            {
                return ResponseUtils.Error<UserDto>(204,
                    MessageKeys.UserNotFound);
            }

            return ResponseUtils.Success(MapToDto(user));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting user by ID {UserId}", userId);
            return ResponseUtils.Error<UserDto>(500,
                MessageKeys.InternalServerError);
        }
    }

    public async Task<Response<UserDto>> GetUserByEmailAsync(string email)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(email))
            {
                return ResponseUtils.Error<UserDto>(400,
                    MessageKeys.EmailRequired);
            }

            var user = await userRepository
                .FirstOrDefaultAsync(u => u.Email == email);

            if (user == null)
            {
                return ResponseUtils.Error<UserDto>(204,
                    MessageKeys.UserNotFound);
            }

            return ResponseUtils.Success(MapToDto(user));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting user by email {Email}", email);
            return ResponseUtils.Error<UserDto>(500,
                MessageKeys.InternalServerError);
        }
    }

    public async Task<ResponseCollection<UserDto>> GetUsersAsync(
        int pageNumber = 1,
        int pageSize = 10,
        string? searchTerm = null)
    {
        try
        {
            var rs = await userRepository.GetAllAsync(
                predicate: p => !string.IsNullOrEmpty(p.Email) && !string.IsNullOrWhiteSpace(searchTerm) && p.Email.Contains(searchTerm) ||
                                !string.IsNullOrEmpty(p.UserName) && !string.IsNullOrWhiteSpace(searchTerm) && p.UserName.Contains(searchTerm),
                projector: p => p.Select(c => new UserDto()
                {
                    Id = c.Id,
                    Email = c.Email,
                    Username = c.UserName ?? c.Email,
                    FirstName = c.FirstName ?? string.Empty,
                    LastName = c.LastName ?? string.Empty,
                    PhoneNumber = c.PhoneNumber,
                    EmailConfirmed = c.EmailConfirmed,
                    PhoneNumberConfirmed = c.PhoneNumberConfirmed,
                    TwoFactorEnabled = c.TwoFactorEnabled,
                    CreatedAt = c.CreatedAt,
                    UpdatedAt = c.UpdatedAt,
                    Provider = c.Provider ?? string.Empty
                }),
                paging: new Pagination(default, pageNumber, pageSize)
            );

            if (rs.Entities == null || !rs.Entities.Any())
            {
                return ResponseUtils.Success<UserDto>([], 204, MessageKeys.NotFound, rs.Pagination);
            }

            return ResponseUtils.Success(rs.Entities, 200, MessageKeys.Success, rs.Pagination);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting paginated users");
            return ResponseUtils.Error<UserDto>([], 500, MessageKeys.InternalServerError);
        }
    }

    public async Task<Response<UserDto>> UpdateUserAsync(Guid userId, UpdateUserDto updateUserDto)
    {
        try
        {
            var user = await userRepository
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null)
            {
                return ResponseUtils.Error<UserDto>(204,
                    MessageKeys.UserNotFound);
            }

            // Check if username is already taken (if updating username)
            if (!string.IsNullOrEmpty(updateUserDto.UserName) && updateUserDto.UserName != user.UserName)
            {
                var existingUsername = await userRepository
                    .AnyAsync(u => !string.IsNullOrEmpty(u.UserName) && u.UserName == updateUserDto.UserName && u.Id != userId);

                if (existingUsername)
                {
                    return ResponseUtils.Error<UserDto>(
                      400, MessageKeys.UserNameTaken);
                }
            }

            // Update user properties
            if (!string.IsNullOrEmpty(updateUserDto.FirstName))
                user.FirstName = updateUserDto.FirstName;

            if (!string.IsNullOrEmpty(updateUserDto.LastName))
                user.LastName = updateUserDto.LastName;

            if (!string.IsNullOrEmpty(updateUserDto.UserName))
                user.UserName = updateUserDto.UserName;

            if (!string.IsNullOrEmpty(updateUserDto.PhoneNumber))
                user.PhoneNumber = updateUserDto.PhoneNumber;

            user.UpdatedAt = DateTimeOffset.UtcNow;
            user.UpdatedBy = userId;
            user.ConcurrencyStamp = Guid.NewGuid().ToString();

            await commandRepository.UpdateAsync(user);
            await commandRepository.SaveChangesAsync(default);

            return ResponseUtils.Success(MapToDto(user), MessageKeys.Success);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error updating user {UserId}", userId);
            return ResponseUtils.Error<UserDto>(500,
                MessageKeys.Error);
        }
    }

    public async Task<Response<bool>> UpdatePasswordAsync(Guid userId, UpdatePasswordDto updatePasswordDto)
    {
        try
        {
            var user = await userRepository
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null)
            {
                return ResponseUtils.Error<bool>(
                   204, MessageKeys.UserNotFound);
            }

            // Verify current password
            if (!passwordHasher.VerifyPassword(updatePasswordDto.CurrentPassword, user.PasswordHash ?? string.Empty))
            {
                return ResponseUtils.Error<bool>(
                   400,
                    MessageKeys.CurrentPasswordIncorrect);
            }

            // Update password
            user.PasswordHash = passwordHasher.HashPassword(updatePasswordDto.NewPassword);
            user.SecurityStamp = Guid.NewGuid().ToString();
            user.UpdatedAt = DateTimeOffset.UtcNow;
            user.UpdatedBy = userId;
            user.ConcurrencyStamp = Guid.NewGuid().ToString();

            await commandRepository.UpdateAsync(user);
            await commandRepository.SaveChangesAsync(default);

            return ResponseUtils.Success(true);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error updating password for user {UserId}", userId);
            return ResponseUtils.Error<bool>(500, MessageKeys.InternalServerError);
        }
    }

    public async Task<Response<IEnumerable<RoleDto>>> GetUserRolesAsync(Guid userId)
    {
        try
        {
            var rolesEntities = await userRoleRepository
                .GetAllAsync(ur => ur.UserId == userId);

            var roles = rolesEntities.Entities
                .Select(r => new RoleDto
                {
                    Id = r.Id,
                    Name = r.Role.Name,
                    Description = r.Role.Description,
                    NormalizedName = r.Role.NormalizedName ?? r.Role.Name.ToUpperInvariant()
                })
                .Cast<RoleDto>()
                .ToList();

            return ResponseUtils.Success<IEnumerable<RoleDto>>(roles);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting roles for user {UserId}", userId);
            return ResponseUtils.Error<IEnumerable<RoleDto>>(500,
                "Error retrieving user roles");
        }
    }

    public async Task<Response<IEnumerable<PermissionDto>>> GetUserPermissionsAsync(Guid userId)
    {
        try
        {
            // Get all roles for user
            var userRoleIdsResult = await userRoleRepository.GetAllAsync(
                ur => ur.UserId == userId,
                projector: p => p.Select(c => c.RoleId)
            );

            var userRoleIds = userRoleIdsResult.Entities.ToList();

            if (userRoleIds.Count == 0)
            {
                return ResponseUtils.Error<IEnumerable<PermissionDto>>(204, "User has no roles assigned");
            }

            // Get all permissions for those roles
            var rolePermissionsResult = await rolePermissionRepository.GetAllAsync(
                rp => userRoleIds.Contains(rp.RoleId),
                projector: p => p.Select(rp => rp.Permission)
            );

            var permissions = rolePermissionsResult.Entities
                .Select(p => new PermissionDto
                {
                    Id = p.Id,
                    Module = p.Module,
                    Action = p.Action,
                    Resource = p.Resource,
                    Description = p.Description
                })
                .Distinct()
                .ToList();

            return ResponseUtils.Success<IEnumerable<PermissionDto>>(permissions);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting permissions for user {UserId}", userId);
            return ResponseUtils.Error<IEnumerable<PermissionDto>>(500,
                "Error retrieving user permissions");
        }
    }

    public async Task<Response<bool>> AssignRoleToUserAsync(Guid userId, Guid roleId)
    {
        try
        {
            // Check if user exists
            var user = await userRepository
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null)
            {
                return ResponseUtils.Error<bool>(204,
                    MessageKeys.UserNotFound);
            }

            // Check if role exists
            var role = await roleRepository
                .FirstOrDefaultAsync(r => r.Id == roleId);

            if (role == null)
            {
                return ResponseUtils.Error<bool>(204,
                   MessageKeys.RoleNotFound);
            }

            // Check if user already has this role
            var existingUserRole = await userRoleRepository
                .FirstOrDefaultAsync(ur => ur.UserId == userId && ur.RoleId == roleId);

            if (existingUserRole != null)
            {
                return ResponseUtils.Success(true); // User already has this role
            }

            // Assign role to user
            var userRole = new UserRole
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                RoleId = roleId,
                CreatedAt = DateTimeOffset.UtcNow,
                CreatedBy = userId
            };

            await commandRepository.AddAsync(userRole);
            await commandRepository.SaveChangesAsync(default);

            return ResponseUtils.Success(true);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error assigning role {RoleId} to user {UserId}", roleId, userId);
            return ResponseUtils.Error<bool>(500, MessageKeys.InternalServerError);
        }
    }

    public async Task<Response<bool>> RemoveRoleFromUserAsync(Guid userId, Guid roleId)
    {
        try
        {
            // Find user role
            var userRole = await userRoleRepository
                .FirstOrDefaultAsync(ur => ur.UserId == userId && ur.RoleId == roleId);

            if (userRole == null)
            {
                return ResponseUtils.Success(true); // User doesn't have this role
            }

            // Remove role from user
            await commandRepository.DeleteAsync(userRole);
            await commandRepository.SaveChangesAsync(default);

            return ResponseUtils.Success(true);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error removing role {RoleId} from user {UserId}", roleId, userId);
            return ResponseUtils.Error<bool>(500, MessageKeys.InternalServerError);
        }
    }

    public async Task<Response<bool>> DeleteUserAsync(Guid userId)
    {
        try
        {
            // Find user
            var user = await userRepository
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null)
            {
                return ResponseUtils.Error<bool>(204,
                    "User not found");
            }

            // Soft delete user
            user.IsDeleted = true;
            user.DeletedAt = DateTimeOffset.UtcNow;
            user.DeletedBy = userId;

            await commandRepository.UpdateAsync(user);
            await commandRepository.SaveChangesAsync(default);

            return ResponseUtils.Success(true);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error deleting user {UserId}", userId);
            return ResponseUtils.Error<bool>(500, MessageKeys.InternalServerError);
        }
    }

    public async Task<bool> HasPermissionAsync(Guid userId, string module, string action, string? resource = null)
    {
        try
        {
            // Get all roles for user
            var userRoleIdsResult = await userRoleRepository.GetAllAsync(
                ur => ur.UserId == userId
            );
            var userRoleIds = userRoleIdsResult.Entities.Select(ur => ur.RoleId).ToList();

            if (userRoleIds.Count == 0)
            {
                return false;
            }

            // Check if any role has the specified permission
            var hasPermission = await rolePermissionRepository.AnyAsync(p =>
                userRoleIds.Contains(p.RoleId) &&
                p.Permission.Module == module &&
                p.Permission.Action == action &&
                (string.IsNullOrEmpty(resource) || p.Permission.Resource == resource || p.Permission.Resource == "*"));

            return hasPermission;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error checking permission for user {UserId}, module {Module}, action {Action}, resource {Resource}",
                userId, module, action, resource);
            return false;
        }
    }

    // Helper method to map User entity to UserDto
    private static UserDto MapToDto(User user)
    {
        return new UserDto
        {
            Id = user.Id,
            Email = user.Email,
            Username = user.UserName ?? user.Email,
            FirstName = user.FirstName ?? string.Empty,
            LastName = user.LastName ?? string.Empty,
            PhoneNumber = user.PhoneNumber,
            EmailConfirmed = user.EmailConfirmed,
            PhoneNumberConfirmed = user.PhoneNumberConfirmed,
            TwoFactorEnabled = user.TwoFactorEnabled,
            CreatedAt = user.CreatedAt,
            UpdatedAt = user.UpdatedAt,
            Provider = user.Provider ?? string.Empty
        };
    }
}