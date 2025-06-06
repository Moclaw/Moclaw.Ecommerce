using Ecom.Users.Domain.Constants;
using Ecom.Users.Domain.DTOs;
using Ecom.Users.Domain.Entities;
using Ecom.Users.Domain.Interfaces;
using EfCore.IRepositories;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using Services.Autofac.Attributes;
using Shared.Utils;
using System.Security.Claims;
using Ecom.Users.Domain.DTOs.Users;

namespace Ecom.Users.Application.Services;
[TransientService]
public class AuthService(
    [FromKeyedServices(ServiceKeys.CommandRepository)] ICommandRepository commandRepository,
    [FromKeyedServices(ServiceKeys.QueryRepository)] IQueryRepository<User, Guid> userRepository,
    [FromKeyedServices(ServiceKeys.QueryRepository)] IQueryRepository<Role, Guid> roleRepository,
    [FromKeyedServices(ServiceKeys.QueryRepository)] IQueryRepository<RefreshToken, Guid> refreshTokenRepository,
    IJwtService jwtService,
    IPasswordHasher passwordHasher,
    ILogger<AuthService> logger) : IAuthService
{
    public async Task<Response<AuthResponseDto>> RegisterAsync(RegisterDto registerDto)
    {
        try
        {
            // Check if email already used
            var existingUser = await userRepository.FirstOrDefaultAsync(u => u.Email == registerDto.Email);
            if (existingUser != null)
            {
                return ResponseUtils.Error<AuthResponseDto>(409, MessageKeys.EmailAlreadyExists);
            }

            // Check username availability
            if (!string.IsNullOrEmpty(registerDto.UserName))
            {
                var existingUsername = await userRepository.FirstOrDefaultAsync(u => u.UserName == registerDto.UserName);
                if (existingUsername != null)
                {
                    return ResponseUtils.Error<AuthResponseDto>(400, MessageKeys.UserNameTaken);
                }
            }

            // Create new user
            var user = new User
            {
                Id = Guid.NewGuid(),
                Email = registerDto.Email,
                UserName = registerDto.UserName ?? registerDto.Email,
                FirstName = registerDto.FirstName,
                LastName = registerDto.LastName,
                PhoneNumber = registerDto.PhoneNumber,
                PasswordHash = passwordHasher.HashPassword(registerDto.Password),
                SecurityStamp = Guid.NewGuid().ToString(),
                ConcurrencyStamp = Guid.NewGuid().ToString(),
                Provider = AuthConstants.Providers.Local,
                CreatedAt = DateTimeOffset.UtcNow,
                CreatedBy = Guid.Empty // fix: must be a Guid
            };

            // Find default user role
            var defaultRole = await roleRepository.FirstOrDefaultAsync(r => r.Name == AuthConstants.Roles.User);
            if (defaultRole == null)
            {
                // Create a default role if it doesn't exist
                defaultRole = new Role
                {
                    Id = Guid.NewGuid(),
                    Name = AuthConstants.Roles.User,
                    NormalizedName = AuthConstants.Roles.User.ToUpper(),
                    Description = "Default user role",
                    ConcurrencyStamp = Guid.NewGuid().ToString(),
                    CreatedAt = DateTimeOffset.UtcNow,
                    CreatedBy = Guid.Empty
                };
                await commandRepository.AddAsync(defaultRole);
            }

            // Assign default role to user
            var userRole = new UserRole
            {
                Id = Guid.NewGuid(),
                UserId = user.Id,
                RoleId = defaultRole.Id,
                CreatedAt = DateTimeOffset.UtcNow,
                CreatedBy = user.Id
            };

            // Add to DB
            await commandRepository.AddAsync(user);
            await commandRepository.AddAsync(userRole);
            await commandRepository.SaveChangesAsync(false, default);

            // Permissions
            var permissions = await GetUserPermissionsAsync(user.Id);

            // Generate tokens
            var jwtId = Guid.NewGuid().ToString();
            var accessToken = jwtService.GenerateAccessToken(user, permissions.Select(p => p.FullPermission));
            var refreshToken = jwtService.GenerateRefreshToken(user, jwtId);

            await commandRepository.AddAsync(refreshToken);
            await commandRepository.SaveChangesAsync(false, default);

            // Return success
            return ResponseUtils.Success(new AuthResponseDto
            {
                UserId = user.Id,
                Email = user.Email,
                UserName = user.UserName ?? user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                AccessToken = accessToken,
                RefreshToken = refreshToken.Token,
                ExpiresAt = DateTimeOffset.UtcNow.AddMinutes(15),
                Roles = [defaultRole.Name],
                Permissions = permissions.Select(p => p.FullPermission)
            }, MessageKeys.Success);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error during user registration");
            return ResponseUtils.Error<AuthResponseDto>(500, MessageKeys.InternalServerError);
        }
    }

    public async Task<Response<AuthResponseDto>> LoginAsync(LoginDto loginDto)
    {
        try
        {
            var user = await userRepository
                .FirstOrDefaultAsync(u => u.Email == loginDto.Email,
                builder: u => u.Include(ur => ur.UserRoles).ThenInclude(ur => ur.Role));

            if (user == null)
            {
                return ResponseUtils.Error<AuthResponseDto>(400, MessageKeys.InvalidCredentials);
            }

            // Check email confirmation
            if (!user.EmailConfirmed)
            {
                return ResponseUtils.Error<AuthResponseDto>(400, MessageKeys.EmailNotConfirmed);
            }

            // Check lockout
            if (user.LockoutEnabled && user.LockoutEnd.HasValue && user.LockoutEnd > DateTimeOffset.UtcNow)
            {
                return ResponseUtils.Error<AuthResponseDto>(401, MessageKeys.AccountLocked);
            }

            // Verify password if local
            if (user.Provider == AuthConstants.Providers.Local)
            {
                if (string.IsNullOrEmpty(user.PasswordHash) ||
                    !passwordHasher.VerifyPassword(loginDto.Password, user.PasswordHash))
                {
                    // Increment failure count
                    user.AccessFailedCount++;
                    if (user.AccessFailedCount >= 5)
                    {
                        user.LockoutEnd = DateTimeOffset.UtcNow.AddMinutes(15);
                    }

                    await commandRepository.UpdateAsync(user);
                    await commandRepository.SaveChangesAsync(false, default);

                    return ResponseUtils.Error<AuthResponseDto>(401, MessageKeys.InvalidCredentials);
                }
            }
            else
            {
                return ResponseUtils.Error<AuthResponseDto>(401, $"This account uses {user.Provider} authentication");
            }

            // Reset failures
            user.AccessFailedCount = 0;
            user.LockoutEnd = null;
            user.UpdatedAt = DateTimeOffset.UtcNow;
            user.UpdatedBy = user.Id;

            await commandRepository.UpdateAsync(user);

            var permissions = await GetUserPermissionsAsync(user.Id);

            var jwtId = Guid.NewGuid().ToString();
            var accessToken = jwtService.GenerateAccessToken(user, permissions.Select(p => p.FullPermission));
            var refreshToken = jwtService.GenerateRefreshToken(user, jwtId);

            await commandRepository.AddAsync(refreshToken);
            await commandRepository.SaveChangesAsync(false, default);

            return ResponseUtils.Success(new AuthResponseDto
            {
                UserId = user.Id,
                Email = user.Email,
                UserName = user.UserName ?? user.Email,
                FirstName = user.FirstName ?? string.Empty,
                LastName = user.LastName ?? string.Empty,
                AccessToken = accessToken,
                RefreshToken = refreshToken.Token,
                ExpiresAt = DateTimeOffset.UtcNow.AddMinutes(15),
                Roles = user.UserRoles.Select(ur => ur.Role.Name),
                Permissions = permissions.Select(p => p.FullPermission)
            }, MessageKeys.Success);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error during user login");
            return ResponseUtils.Error<AuthResponseDto>(500, MessageKeys.InternalServerError);
        }
    }

    public async Task<Response<AuthResponseDto>> RefreshTokenAsync(RefreshTokenDto refreshTokenDto)
    {
        try
        {
            var refreshToken = await refreshTokenRepository.FirstOrDefaultAsync(rt => rt.Token == refreshTokenDto.RefreshToken);

            if (refreshToken == null)
            {
                return ResponseUtils.Error<AuthResponseDto>(401, MessageKeys.InvalidToken);
            }

            if (refreshToken.IsUsed || refreshToken.IsRevoked)
            {
                return ResponseUtils.Error<AuthResponseDto>(401, MessageKeys.InvalidToken);
            }

            if (refreshToken.ExpiryDate <= DateTimeOffset.UtcNow)
            {
                return ResponseUtils.Error<AuthResponseDto>(401, MessageKeys.TokenExpired);
            }

            var user = await userRepository
                .FirstOrDefaultAsync(u => u.Id == refreshToken.UserId,
                builder: u => u.Include(ur => ur.UserRoles).ThenInclude(ur => ur.Role)
                );

            if (user == null)
            {
                return ResponseUtils.Error<AuthResponseDto>(404, MessageKeys.UserNotFound);
            }

            refreshToken.IsUsed = true;
            refreshToken.UpdatedAt = DateTimeOffset.UtcNow;
            refreshToken.UpdatedBy = user.Id;

            await commandRepository.UpdateAsync(refreshToken);

            var permissions = await GetUserPermissionsAsync(user.Id);

            var jwtId = Guid.NewGuid().ToString();
            var accessToken = jwtService.GenerateAccessToken(user, permissions.Select(p => p.FullPermission));
            var newRefreshToken = jwtService.GenerateRefreshToken(user, jwtId);

            await commandRepository.AddAsync(newRefreshToken);
            await commandRepository.SaveChangesAsync(false, default);

            return ResponseUtils.Success(new AuthResponseDto
            {
                UserId = user.Id,
                Email = user.Email,
                UserName = user.UserName ?? user.Email,
                FirstName = user?.FirstName ?? string.Empty,
                LastName = user?.LastName ?? string.Empty,
                AccessToken = accessToken,
                RefreshToken = newRefreshToken.Token,
                ExpiresAt = DateTimeOffset.UtcNow.AddMinutes(15),
                Roles = user?.UserRoles?.Select(ur => ur.Role.Name) ?? [],
                Permissions = permissions.Select(p => p.FullPermission)
            }, MessageKeys.Success);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error during token refresh");
            return ResponseUtils.Error<AuthResponseDto>(500, MessageKeys.InternalServerError);
        }
    }

    public async Task<Response<AuthResponseDto>> HandleOAuthCallbackAsync(string provider, string code)
    {
        // Placeholder. Not truly using async calls yet
        await Task.CompletedTask;

        logger.LogInformation("OAuth callback received for provider: {Provider} with code: {Code}", provider, code);
        return ResponseUtils.Error<AuthResponseDto>(501, "OAuth authentication is not fully implemented in this version");
    }

    public async Task<Response<bool>> RevokeAllRefreshTokensAsync(Guid userId)
    {
        try
        {
            var refreshTokens = await refreshTokenRepository
                .GetAllAsync(rt => rt.UserId == userId && !rt.IsUsed && !rt.IsRevoked && rt.ExpiryDate > DateTimeOffset.UtcNow);

            if (!refreshTokens.Entities.Any())
            {
                return ResponseUtils.Success(true);
            }

            foreach (var token in refreshTokens.Entities)
            {
                token.IsRevoked = true;
                token.RevokedAt = DateTimeOffset.UtcNow;
                token.UpdatedAt = DateTimeOffset.UtcNow;
                token.UpdatedBy = userId;
                await commandRepository.UpdateAsync(token);
            }

            await commandRepository.SaveChangesAsync(false, default);
            return ResponseUtils.Success(true);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error revoking all refresh tokens for user {UserId}", userId);
            return ResponseUtils.Error<bool>(500, "Token revocation failed: An unexpected error occurred during token revocation");
        }
    }

    public async Task<Response<bool>> RevokeRefreshTokenAsync(string token)
    {
        try
        {
            var refreshToken = await refreshTokenRepository.FirstOrDefaultAsync(rt => rt.Token == token);

            if (refreshToken == null)
            {
                return ResponseUtils.Error<bool>(400, "Token revocation failed: Invalid refresh token");
            }

            refreshToken.IsRevoked = true;
            refreshToken.RevokedAt = DateTimeOffset.UtcNow;
            refreshToken.UpdatedAt = DateTimeOffset.UtcNow;
            refreshToken.UpdatedBy = refreshToken.UserId;

            await commandRepository.UpdateAsync(refreshToken);
            await commandRepository.SaveChangesAsync(false, default);

            return ResponseUtils.Success(true);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error revoking refresh token");
            return ResponseUtils.Error<bool>(500, "Token revocation failed: An unexpected error occurred during token revocation");
        }
    }

    public async Task<Response<bool>> ValidateTokenAsync(string token)
    {
        await Task.CompletedTask; // fix potential CS1998
        try
        {
            var isValid = jwtService.ValidateToken(token);
            return ResponseUtils.Success(isValid);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error validating token");
            return ResponseUtils.Error<bool>(500, "Token validation failed: An unexpected error occurred during token validation");
        }
    }

    public async Task<(string AccessToken, string RefreshToken)> GenerateTokensAsync(User user)
    {
        var permissions = await GetUserPermissionsAsync(user.Id);
        var jwtId = Guid.NewGuid().ToString();
        var accessToken = jwtService.GenerateAccessToken(user, permissions.Select(p => p.FullPermission));
        var refreshToken = jwtService.GenerateRefreshToken(user, jwtId);

        await commandRepository.AddAsync(refreshToken);
        await commandRepository.SaveChangesAsync(false, default);

        return (accessToken, refreshToken.Token);
    }

    public async Task<User?> AuthenticateGoogleAsync(string googleToken)
    {
        await Task.CompletedTask;
        logger.LogInformation("Google authentication requested with token: {Token}", googleToken);
        // TODO: Implement Google authentication
        return null;
    }

    public async Task<User?> AuthenticateFacebookAsync(string facebookToken)
    {
        await Task.CompletedTask;
        logger.LogInformation("Facebook authentication requested with token: {Token}", facebookToken);
        // TODO: Implement Facebook authentication
        return null;
    }

    public async Task<ClaimsPrincipal?> GetPrincipalFromTokenAsync(string token)
    {
        await Task.CompletedTask;
        try
        {
            return jwtService.GetPrincipalFromExpiredToken(token);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting principal from token");
            return null;
        }
    }

    public async Task<Response<AuthResponseDto>> SSOLoginAsync(string provider, string token)
    {
        try
        {
            User? user = null;

            // Authenticate based on provider
            switch (provider.ToLower())
            {
                case "google":
                    user = await AuthenticateGoogleAsync(token);
                    break;
                case "facebook":
                    user = await AuthenticateFacebookAsync(token);
                    break;
                default:
                    return ResponseUtils.Error<AuthResponseDto>(400, $"SSO Login failed: Unsupported provider '{provider}'");
            }

            if (user == null)
            {
                return ResponseUtils.Error<AuthResponseDto>(401, "SSO Login failed: Invalid token or authentication failed");
            }

            // Check if user exists in database
            var existingUser = await userRepository.FirstOrDefaultAsync(u => u.Email == user.Email,
                builder: u => u.Include(ur => ur.UserRoles).ThenInclude(ur => ur.Role));

            if (existingUser == null)
            {
                // Create new user from SSO
                var newUser = new User
                {
                    Id = Guid.NewGuid(),
                    Email = user.Email,
                    UserName = user.Email,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    Provider = provider,
                    SecurityStamp = Guid.NewGuid().ToString(),
                    ConcurrencyStamp = Guid.NewGuid().ToString(),
                    CreatedAt = DateTimeOffset.UtcNow,
                    CreatedBy = Guid.Empty
                };

                // Assign default role
                var defaultRole = await roleRepository.FirstOrDefaultAsync(r => r.Name == AuthConstants.Roles.User);
                if (defaultRole != null)
                {
                    var userRole = new UserRole
                    {
                        Id = Guid.NewGuid(),
                        UserId = newUser.Id,
                        RoleId = defaultRole.Id,
                        CreatedAt = DateTimeOffset.UtcNow,
                        CreatedBy = newUser.Id
                    };

                    await commandRepository.AddAsync(newUser);
                    await commandRepository.AddAsync(userRole);
                    await commandRepository.SaveChangesAsync(false, default);
                }

                existingUser = newUser;
            }

            // Generate tokens
            var permissions = await GetUserPermissionsAsync(existingUser.Id);
            var jwtId = Guid.NewGuid().ToString();
            var accessToken = jwtService.GenerateAccessToken(existingUser, permissions.Select(p => p.FullPermission));
            var refreshToken = jwtService.GenerateRefreshToken(existingUser, jwtId);

            await commandRepository.AddAsync(refreshToken);
            await commandRepository.SaveChangesAsync(false, default);

            return ResponseUtils.Success(new AuthResponseDto
            {
                UserId = existingUser.Id,
                Email = existingUser.Email,
                UserName = existingUser.UserName ?? existingUser.Email,
                FirstName = existingUser.FirstName ?? string.Empty,
                LastName = existingUser.LastName ?? string.Empty,
                AccessToken = accessToken,
                RefreshToken = refreshToken.Token,
                ExpiresAt = DateTimeOffset.UtcNow.AddMinutes(15),
                Roles = existingUser.UserRoles?.Select(ur => ur.Role.Name) ?? [],
                Permissions = permissions.Select(p => p.FullPermission)
            });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error during SSO login for provider: {Provider}", provider);
            return ResponseUtils.Error<AuthResponseDto>(500, "SSO Login failed: An unexpected error occurred during authentication");
        }
    }

    public async Task<Response<bool>> UpdateUserAsync(Guid currentUserId, Guid targetUserId, UpdateUserDto updateUserDto, bool isAdmin = false)
    {
        try
        {
            // Permission check: Admin can update anyone, regular users can only update themselves
            if (!isAdmin && currentUserId != targetUserId)
            {
                return ResponseUtils.Error<bool>(403, MessageKeys.AccessDenied);
            }

            // Get target user
            var targetUser = await userRepository.FirstOrDefaultAsync(u => u.Id == targetUserId);
            if (targetUser == null)
            {
                return ResponseUtils.Error<bool>(404, MessageKeys.UserNotFound);
            }

            // Check if email is being changed and if it's already in use
            if (!string.IsNullOrEmpty(updateUserDto.Email) && updateUserDto.Email != targetUser.Email)
            {
                var existingUser = await userRepository.FirstOrDefaultAsync(u => u.Email == updateUserDto.Email && u.Id != targetUserId);
                if (existingUser != null)
                {
                    return ResponseUtils.Error<bool>(400, MessageKeys.EmailAlreadyExists);
                }
                targetUser.Email = updateUserDto.Email;
            }

            // Check if username is being changed and if it's already in use
            if (!string.IsNullOrEmpty(updateUserDto.UserName) && updateUserDto.UserName != targetUser.UserName)
            {
                var existingUsername = await userRepository.FirstOrDefaultAsync(u => u.UserName == updateUserDto.UserName && u.Id != targetUserId);
                if (existingUsername != null)
                {
                    return ResponseUtils.Error<bool>(400, MessageKeys.UserNameTaken);
                }
                targetUser.UserName = updateUserDto.UserName;
            }

            // Update basic information
            if (!string.IsNullOrEmpty(updateUserDto.FirstName))
                targetUser.FirstName = updateUserDto.FirstName;

            if (!string.IsNullOrEmpty(updateUserDto.LastName))
                targetUser.LastName = updateUserDto.LastName;

            if (!string.IsNullOrEmpty(updateUserDto.PhoneNumber))
                targetUser.PhoneNumber = updateUserDto.PhoneNumber;

            // Update password if provided (only for local accounts)
            if (!string.IsNullOrEmpty(updateUserDto.NewPassword))
            {
                if (targetUser.Provider != AuthConstants.Providers.Local)
                {
                    return ResponseUtils.Error<bool>(400, "Cannot change password for SSO accounts");
                }

                // Verify current password if not admin
                if (!isAdmin)
                {
                    if (string.IsNullOrEmpty(updateUserDto.CurrentPassword) ||
                        string.IsNullOrEmpty(targetUser.PasswordHash) ||
                        !passwordHasher.VerifyPassword(updateUserDto.CurrentPassword, targetUser.PasswordHash))
                    {
                        return ResponseUtils.Error<bool>(400, MessageKeys.CurrentPasswordIncorrect);
                    }
                }

                targetUser.PasswordHash = passwordHasher.HashPassword(updateUserDto.NewPassword);
                targetUser.SecurityStamp = Guid.NewGuid().ToString();
            }

            // Update audit fields
            targetUser.UpdatedAt = DateTimeOffset.UtcNow;
            targetUser.UpdatedBy = currentUserId;

            await commandRepository.UpdateAsync(targetUser);
            await commandRepository.SaveChangesAsync(false, default);

            logger.LogInformation("User {TargetUserId} updated by {CurrentUserId}", targetUserId, currentUserId);
            return ResponseUtils.Success(true, MessageKeys.Success);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error updating user {TargetUserId} by {CurrentUserId}", targetUserId, currentUserId);
            return ResponseUtils.Error<bool>(500, MessageKeys.Error);
        }
    }

    private async Task<List<PermissionDto>> GetUserPermissionsAsync(Guid userId)
    {
        var userRolesAll = await userRepository
            .GetAllAsync(u => u.Id == userId, builder: u => u.Include(ur => ur.UserRoles).ThenInclude(ur => ur.Role));


        if (!userRolesAll.Entities.Any())
        {
            return [];
        }

        var userRoles = userRolesAll.Entities.First().UserRoles.Select(ur => ur.Role.Id).ToList();

        var permissionsAll = await roleRepository.GetAllAsync(
                r => userRoles.Contains(r.Id)
            );

        if (!permissionsAll.Entities.Any())
        {
            return [];
        }

        var permissions = permissionsAll.Entities
            .SelectMany(r => r.RolePermissions)
            .Select(p => new PermissionDto
            {
                Id = p.Permission.Id,
                Action = p.Permission.Action,
                Description = p.Permission.Description,
                Module = p.Permission.Module,
                Resource = p.Permission.Resource,
            })
            .Distinct()
            .ToList();

        return permissions;
    }
}