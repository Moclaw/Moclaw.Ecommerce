namespace Ecom.Users.Infrastructure.Services;

public class UserService : IUserService
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ILogger<UserService> _logger;

    public UserService(
        IUserRepository userRepository,
        IPasswordHasher passwordHasher,
        ILogger<UserService> logger)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
        _logger = logger;
    }

    public async Task<UserDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetByIdWithRolesAsync(id, cancellationToken);
        
        if (user == null)
            return null;

        var roles = await _userRepository.GetUserRolesAsync(user.Id, cancellationToken);
        var permissions = await _userRepository.GetUserPermissionsAsync(user.Id, cancellationToken);

        return new UserDto
        {
            Id = user.Id,
            Email = user.Email,
            Username = user.Username,
            FirstName = user.FirstName,
            LastName = user.LastName,
            PhoneNumber = user.PhoneNumber,
            IsActive = user.IsActive,
            EmailConfirmed = user.EmailConfirmed,
            PhoneNumberConfirmed = user.PhoneNumberConfirmed,
            TwoFactorEnabled = user.TwoFactorEnabled,
            ProfilePictureUrl = user.ProfilePictureUrl,
            CreatedAt = user.CreatedAt,
            UpdatedAt = user.UpdatedAt,
            LastLoginAt = user.LastLoginAt,
            Roles = roles,
            Permissions = permissions
        };
    }

    public async Task<UserDto?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetByEmailWithRolesAsync(email, cancellationToken);
        
        if (user == null)
            return null;

        var roles = await _userRepository.GetUserRolesAsync(user.Id, cancellationToken);
        var permissions = await _userRepository.GetUserPermissionsAsync(user.Id, cancellationToken);

        return new UserDto
        {
            Id = user.Id,
            Email = user.Email,
            Username = user.Username,
            FirstName = user.FirstName,
            LastName = user.LastName,
            PhoneNumber = user.PhoneNumber,
            IsActive = user.IsActive,
            EmailConfirmed = user.EmailConfirmed,
            PhoneNumberConfirmed = user.PhoneNumberConfirmed,
            TwoFactorEnabled = user.TwoFactorEnabled,
            ProfilePictureUrl = user.ProfilePictureUrl,
            CreatedAt = user.CreatedAt,
            UpdatedAt = user.UpdatedAt,
            LastLoginAt = user.LastLoginAt,
            Roles = roles,
            Permissions = permissions
        };
    }

    public async Task<PagedResult<UserDto>> GetUsersAsync(int page, int pageSize, string? searchTerm = null, CancellationToken cancellationToken = default)
    {
        var result = await _userRepository.GetPagedAsync(page, pageSize, searchTerm, cancellationToken);
        
        var userDtos = new List<UserDto>();
        
        foreach (var user in result.Items)
        {
            var roles = await _userRepository.GetUserRolesAsync(user.Id, cancellationToken);
            var permissions = await _userRepository.GetUserPermissionsAsync(user.Id, cancellationToken);
            
            userDtos.Add(new UserDto
            {
                Id = user.Id,
                Email = user.Email,
                Username = user.Username,
                FirstName = user.FirstName,
                LastName = user.LastName,
                PhoneNumber = user.PhoneNumber,
                IsActive = user.IsActive,
                EmailConfirmed = user.EmailConfirmed,
                PhoneNumberConfirmed = user.PhoneNumberConfirmed,
                TwoFactorEnabled = user.TwoFactorEnabled,
                ProfilePictureUrl = user.ProfilePictureUrl,
                CreatedAt = user.CreatedAt,
                UpdatedAt = user.UpdatedAt,
                LastLoginAt = user.LastLoginAt,
                Roles = roles,
                Permissions = permissions
            });
        }

        return new PagedResult<UserDto>
        {
            Items = userDtos,
            TotalCount = result.TotalCount,
            Page = result.Page,
            PageSize = result.PageSize
        };
    }

    public async Task<UserDto> UpdateAsync(int id, UpdateUserDto dto, CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetByIdAsync(id, cancellationToken);
        
        if (user == null)
        {
            throw new NotFoundException($"User with ID {id} not found");
        }

        // Check if email is being changed and if it's already taken
        if (!string.Equals(user.Email, dto.Email, StringComparison.OrdinalIgnoreCase))
        {
            if (await _userRepository.ExistsAsync(dto.Email, cancellationToken: cancellationToken))
            {
                throw new InvalidOperationException("Email is already taken");
            }
            user.Email = dto.Email.ToLowerInvariant();
            user.EmailConfirmed = false; // Reset email confirmation when email changes
        }

        // Check if username is being changed and if it's already taken
        if (!string.Equals(user.Username, dto.Username, StringComparison.OrdinalIgnoreCase))
        {
            if (!string.IsNullOrEmpty(dto.Username) && await _userRepository.ExistsAsync(user.Email, dto.Username, cancellationToken))
            {
                throw new InvalidOperationException("Username is already taken");
            }
            user.Username = dto.Username?.ToLowerInvariant();
        }

        // Update other properties
        user.FirstName = dto.FirstName;
        user.LastName = dto.LastName;
        user.PhoneNumber = dto.PhoneNumber;
        user.UpdatedAt = DateTime.UtcNow;

        // Reset phone confirmation if phone number changes
        if (!string.Equals(user.PhoneNumber, dto.PhoneNumber))
        {
            user.PhoneNumberConfirmed = false;
        }

        await _userRepository.UpdateAsync(user, cancellationToken);

        _logger.LogInformation("User {UserId} updated successfully", user.Id);

        // Return updated user
        return await GetByIdAsync(user.Id, cancellationToken) ?? throw new InvalidOperationException("Failed to retrieve updated user");
    }

    public async Task<bool> ChangePasswordAsync(int id, ChangePasswordDto dto, CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetByIdAsync(id, cancellationToken);
        
        if (user == null)
        {
            throw new NotFoundException($"User with ID {id} not found");
        }

        // Verify current password
        if (!_passwordHasher.VerifyPassword(dto.CurrentPassword, user.PasswordHash))
        {
            throw new UnauthorizedAccessException("Current password is incorrect");
        }

        // Hash and set new password
        user.PasswordHash = _passwordHasher.HashPassword(dto.NewPassword);
        user.UpdatedAt = DateTime.UtcNow;

        await _userRepository.UpdateAsync(user, cancellationToken);

        _logger.LogInformation("Password changed for user {UserId}", user.Id);

        return true;
    }

    public async Task<List<string>> GetUserPermissionsAsync(int userId, CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetByIdAsync(userId, cancellationToken);
        
        if (user == null)
        {
            throw new NotFoundException($"User with ID {userId} not found");
        }

        return await _userRepository.GetUserPermissionsAsync(userId, cancellationToken);
    }

    public async Task<List<string>> GetUserRolesAsync(int userId, CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetByIdAsync(userId, cancellationToken);
        
        if (user == null)
        {
            throw new NotFoundException($"User with ID {userId} not found");
        }

        return await _userRepository.GetUserRolesAsync(userId, cancellationToken);
    }

    public async Task<bool> DeactivateAsync(int id, CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetByIdAsync(id, cancellationToken);
        
        if (user == null)
        {
            throw new NotFoundException($"User with ID {id} not found");
        }

        user.IsActive = false;
        user.UpdatedAt = DateTime.UtcNow;

        await _userRepository.UpdateAsync(user, cancellationToken);

        _logger.LogInformation("User {UserId} deactivated", user.Id);

        return true;
    }

    public async Task<bool> ActivateAsync(int id, CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetByIdAsync(id, cancellationToken);
        
        if (user == null)
        {
            throw new NotFoundException($"User with ID {id} not found");
        }

        user.IsActive = true;
        user.UpdatedAt = DateTime.UtcNow;

        await _userRepository.UpdateAsync(user, cancellationToken);

        _logger.LogInformation("User {UserId} activated", user.Id);

        return true;
    }

    public async Task<bool> UpdateProfilePictureAsync(int id, string profilePictureUrl, CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetByIdAsync(id, cancellationToken);
        
        if (user == null)
        {
            throw new NotFoundException($"User with ID {id} not found");
        }

        user.ProfilePictureUrl = profilePictureUrl;
        user.UpdatedAt = DateTime.UtcNow;

        await _userRepository.UpdateAsync(user, cancellationToken);

        _logger.LogInformation("Profile picture updated for user {UserId}", user.Id);

        return true;
    }
}
