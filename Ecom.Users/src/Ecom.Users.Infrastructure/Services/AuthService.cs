namespace Ecom.Users.Infrastructure.Services;

public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepository;
    private readonly IRoleRepository _roleRepository;
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly IJwtService _jwtService;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ILogger<AuthService> _logger;

    public AuthService(
        IUserRepository userRepository,
        IRoleRepository roleRepository,
        IRefreshTokenRepository refreshTokenRepository,
        IJwtService jwtService,
        IPasswordHasher passwordHasher,
        ILogger<AuthService> logger)
    {
        _userRepository = userRepository;
        _roleRepository = roleRepository;
        _refreshTokenRepository = refreshTokenRepository;
        _jwtService = jwtService;
        _passwordHasher = passwordHasher;
        _logger = logger;
    }

    public async Task<AuthResultDto> LoginAsync(LoginDto dto, CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetByEmailWithRolesAsync(dto.Email, cancellationToken);
        
        if (user == null || !_passwordHasher.VerifyPassword(dto.Password, user.PasswordHash))
        {
            _logger.LogWarning("Login attempt failed for email: {Email}", dto.Email);
            throw new UnauthorizedAccessException("Invalid email or password");
        }

        if (!user.IsActive)
        {
            _logger.LogWarning("Login attempt for inactive user: {Email}", dto.Email);
            throw new UnauthorizedAccessException("Account is inactive");
        }

        if (!user.EmailConfirmed)
        {
            _logger.LogWarning("Login attempt for unconfirmed email: {Email}", dto.Email);
            throw new UnauthorizedAccessException("Email not confirmed");
        }

        // Update last login
        user.LastLoginAt = DateTime.UtcNow;
        await _userRepository.UpdateAsync(user, cancellationToken);

        // Generate tokens
        var permissions = await _userRepository.GetUserPermissionsAsync(user.Id, cancellationToken);
        var roles = await _userRepository.GetUserRolesAsync(user.Id, cancellationToken);
        
        var accessToken = _jwtService.GenerateAccessToken(user, roles, permissions);
        var refreshToken = _jwtService.GenerateRefreshToken(user.Id, dto.DeviceInfo, dto.IpAddress);

        // Save refresh token
        await _refreshTokenRepository.AddAsync(refreshToken, cancellationToken);

        _logger.LogInformation("User {UserId} logged in successfully", user.Id);

        return new AuthResultDto
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken.Token,
            ExpiresIn = _jwtService.GetTokenExpirationMinutes() * 60, // Convert to seconds
            TokenType = "Bearer",
            User = new UserDto
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
                LastLoginAt = user.LastLoginAt,
                Roles = roles,
                Permissions = permissions
            }
        };
    }

    public async Task<AuthResultDto> RegisterAsync(RegisterDto dto, CancellationToken cancellationToken = default)
    {
        // Check if user already exists
        if (await _userRepository.ExistsAsync(dto.Email, dto.Username, cancellationToken))
        {
            throw new InvalidOperationException("User with this email or username already exists");
        }

        // Get default User role
        var userRole = await _roleRepository.GetByNameAsync(AuthConstants.Roles.User, cancellationToken);
        if (userRole == null)
        {
            throw new InvalidOperationException("Default user role not found");
        }

        // Create new user
        var user = new User
        {
            Email = dto.Email.ToLowerInvariant(),
            Username = dto.Username?.ToLowerInvariant(),
            FirstName = dto.FirstName,
            LastName = dto.LastName,
            PhoneNumber = dto.PhoneNumber,
            PasswordHash = _passwordHasher.HashPassword(dto.Password),
            IsActive = true,
            EmailConfirmed = false, // Will need email confirmation
            PhoneNumberConfirmed = false,
            TwoFactorEnabled = false,
            CreatedAt = DateTime.UtcNow
        };

        await _userRepository.AddAsync(user, cancellationToken);

        // Assign default User role
        var userRoleAssignment = new UserRole
        {
            UserId = user.Id,
            RoleId = userRole.Id,
            AssignedAt = DateTime.UtcNow,
            AssignedBy = user.Id, // Self-assigned during registration
            IsActive = true
        };

        // Add role assignment through context directly since we don't have UserRole repository
        // This would typically be done through a UserRole repository or service
        // For now, we'll handle it in the service layer

        _logger.LogInformation("User {UserId} registered successfully", user.Id);

        // Generate tokens for immediate login after registration
        var permissions = await _userRepository.GetUserPermissionsAsync(user.Id, cancellationToken);
        var roles = new List<string> { AuthConstants.Roles.User };
        
        var accessToken = _jwtService.GenerateAccessToken(user, roles, permissions);
        var refreshToken = _jwtService.GenerateRefreshToken(user.Id, dto.DeviceInfo, dto.IpAddress);

        await _refreshTokenRepository.AddAsync(refreshToken, cancellationToken);

        return new AuthResultDto
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken.Token,
            ExpiresIn = _jwtService.GetTokenExpirationMinutes() * 60,
            TokenType = "Bearer",
            User = new UserDto
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
                LastLoginAt = user.LastLoginAt,
                Roles = roles,
                Permissions = permissions
            }
        };
    }

    public async Task<AuthResultDto> RefreshTokenAsync(RefreshTokenDto dto, CancellationToken cancellationToken = default)
    {
        // Get and validate refresh token
        var refreshToken = await _refreshTokenRepository.GetByTokenAsync(dto.RefreshToken, cancellationToken);
        
        if (refreshToken == null || refreshToken.Used || refreshToken.Invalidated || refreshToken.ExpiresAt < DateTime.UtcNow)
        {
            throw new UnauthorizedAccessException("Invalid refresh token");
        }

        // Validate JWT ID if provided
        if (!string.IsNullOrEmpty(dto.JwtId))
        {
            var jwtIdFromToken = _jwtService.GetJwtIdFromToken(dto.AccessToken);
            if (refreshToken.JwtId != jwtIdFromToken)
            {
                // Mark token as invalidated - possible token theft
                refreshToken.Invalidated = true;
                refreshToken.InvalidatedAt = DateTime.UtcNow;
                await _refreshTokenRepository.UpdateAsync(refreshToken, cancellationToken);
                
                _logger.LogWarning("JWT ID mismatch during token refresh for user {UserId}", refreshToken.UserId);
                throw new UnauthorizedAccessException("Invalid token pair");
            }
        }

        // Mark current refresh token as used
        refreshToken.Used = true;
        refreshToken.UsedAt = DateTime.UtcNow;
        await _refreshTokenRepository.UpdateAsync(refreshToken, cancellationToken);

        // Get user with roles
        var user = await _userRepository.GetByIdWithRolesAsync(refreshToken.UserId, cancellationToken);
        if (user == null || !user.IsActive)
        {
            throw new UnauthorizedAccessException("User not found or inactive");
        }

        // Generate new tokens
        var permissions = await _userRepository.GetUserPermissionsAsync(user.Id, cancellationToken);
        var roles = await _userRepository.GetUserRolesAsync(user.Id, cancellationToken);
        
        var newAccessToken = _jwtService.GenerateAccessToken(user, roles, permissions);
        var newRefreshToken = _jwtService.GenerateRefreshToken(user.Id, dto.DeviceInfo, dto.IpAddress);

        await _refreshTokenRepository.AddAsync(newRefreshToken, cancellationToken);

        _logger.LogInformation("Tokens refreshed for user {UserId}", user.Id);

        return new AuthResultDto
        {
            AccessToken = newAccessToken,
            RefreshToken = newRefreshToken.Token,
            ExpiresIn = _jwtService.GetTokenExpirationMinutes() * 60,
            TokenType = "Bearer",
            User = new UserDto
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
                LastLoginAt = user.LastLoginAt,
                Roles = roles,
                Permissions = permissions
            }
        };
    }

    public async Task<AuthResultDto> SocialLoginAsync(SocialLoginDto dto, CancellationToken cancellationToken = default)
    {
        // This would typically validate the social token with the provider (Google, Facebook, etc.)
        // For now, we'll implement a basic structure
        
        // TODO: Implement actual social provider validation
        // var socialUser = await ValidateSocialTokenAsync(dto.Provider, dto.Token, cancellationToken);
        
        // For demonstration, we'll create a mock social user
        var socialEmail = dto.Email; // This would come from the social provider
        
        if (string.IsNullOrEmpty(socialEmail))
        {
            throw new UnauthorizedAccessException("Unable to get email from social provider");
        }

        // Check if user exists
        var existingUser = await _userRepository.GetByEmailWithRolesAsync(socialEmail, cancellationToken);
        
        if (existingUser != null)
        {
            // User exists, proceed with login
            if (!existingUser.IsActive)
            {
                throw new UnauthorizedAccessException("Account is inactive");
            }

            // Update last login
            existingUser.LastLoginAt = DateTime.UtcNow;
            await _userRepository.UpdateAsync(existingUser, cancellationToken);

            var permissions = await _userRepository.GetUserPermissionsAsync(existingUser.Id, cancellationToken);
            var roles = await _userRepository.GetUserRolesAsync(existingUser.Id, cancellationToken);
            
            var accessToken = _jwtService.GenerateAccessToken(existingUser, roles, permissions);
            var refreshToken = _jwtService.GenerateRefreshToken(existingUser.Id, dto.DeviceInfo, dto.IpAddress);

            await _refreshTokenRepository.AddAsync(refreshToken, cancellationToken);

            return new AuthResultDto
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken.Token,
                ExpiresIn = _jwtService.GetTokenExpirationMinutes() * 60,
                TokenType = "Bearer",
                User = new UserDto
                {
                    Id = existingUser.Id,
                    Email = existingUser.Email,
                    Username = existingUser.Username,
                    FirstName = existingUser.FirstName,
                    LastName = existingUser.LastName,
                    PhoneNumber = existingUser.PhoneNumber,
                    IsActive = existingUser.IsActive,
                    EmailConfirmed = existingUser.EmailConfirmed,
                    PhoneNumberConfirmed = existingUser.PhoneNumberConfirmed,
                    TwoFactorEnabled = existingUser.TwoFactorEnabled,
                    ProfilePictureUrl = existingUser.ProfilePictureUrl,
                    CreatedAt = existingUser.CreatedAt,
                    LastLoginAt = existingUser.LastLoginAt,
                    Roles = roles,
                    Permissions = permissions
                }
            };
        }
        else
        {
            // Create new user from social login
            var userRole = await _roleRepository.GetByNameAsync(AuthConstants.Roles.User, cancellationToken);
            if (userRole == null)
            {
                throw new InvalidOperationException("Default user role not found");
            }

            var newUser = new User
            {
                Email = socialEmail.ToLowerInvariant(),
                FirstName = dto.FirstName,
                LastName = dto.LastName,
                ProfilePictureUrl = dto.ProfilePictureUrl,
                IsActive = true,
                EmailConfirmed = true, // Social logins typically have verified emails
                PhoneNumberConfirmed = false,
                TwoFactorEnabled = false,
                CreatedAt = DateTime.UtcNow,
                LastLoginAt = DateTime.UtcNow
            };

            await _userRepository.AddAsync(newUser, cancellationToken);

            var permissions = await _userRepository.GetUserPermissionsAsync(newUser.Id, cancellationToken);
            var roles = new List<string> { AuthConstants.Roles.User };
            
            var accessToken = _jwtService.GenerateAccessToken(newUser, roles, permissions);
            var refreshToken = _jwtService.GenerateRefreshToken(newUser.Id, dto.DeviceInfo, dto.IpAddress);

            await _refreshTokenRepository.AddAsync(refreshToken, cancellationToken);

            _logger.LogInformation("New user {UserId} created via social login ({Provider})", newUser.Id, dto.Provider);

            return new AuthResultDto
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken.Token,
                ExpiresIn = _jwtService.GetTokenExpirationMinutes() * 60,
                TokenType = "Bearer",
                User = new UserDto
                {
                    Id = newUser.Id,
                    Email = newUser.Email,
                    Username = newUser.Username,
                    FirstName = newUser.FirstName,
                    LastName = newUser.LastName,
                    PhoneNumber = newUser.PhoneNumber,
                    IsActive = newUser.IsActive,
                    EmailConfirmed = newUser.EmailConfirmed,
                    PhoneNumberConfirmed = newUser.PhoneNumberConfirmed,
                    TwoFactorEnabled = newUser.TwoFactorEnabled,
                    ProfilePictureUrl = newUser.ProfilePictureUrl,
                    CreatedAt = newUser.CreatedAt,
                    LastLoginAt = newUser.LastLoginAt,
                    Roles = roles,
                    Permissions = permissions
                }
            };
        }
    }

    public async Task LogoutAsync(string refreshToken, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(refreshToken))
            return;

        await _refreshTokenRepository.InvalidateTokenAsync(refreshToken, cancellationToken);
        _logger.LogInformation("User logged out and refresh token invalidated");
    }

    public async Task LogoutAllDevicesAsync(int userId, CancellationToken cancellationToken = default)
    {
        await _refreshTokenRepository.InvalidateAllUserTokensAsync(userId, cancellationToken);
        _logger.LogInformation("All refresh tokens invalidated for user {UserId}", userId);
    }
}
