using Microsoft.Extensions.Configuration;

namespace Ecom.Users.Infrastructure;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
    {
        // Add DbContext
        services.AddDbContext<UsersDbContext>(options =>
        {
            var connectionString = configuration.GetConnectionString("DefaultConnection");
            options.UseSqlServer(connectionString, b => b.MigrationsAssembly(typeof(UsersDbContext).Assembly.FullName));
        });

        // Register repositories
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IRoleRepository, RoleRepository>();
        services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();

        // Register services
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IJwtService, JwtService>();
        services.AddScoped<IPasswordHasher, PasswordHasher>();

        // Configure JWT options
        services.Configure<JwtOptions>(configuration.GetSection("Jwt"));

        // Add JWT authentication
        var jwtOptions = configuration.GetSection("Jwt").Get<JwtOptions>();
        if (jwtOptions == null)
        {
            throw new InvalidOperationException("JWT configuration is missing");
        }

        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            options.SaveToken = true;
            options.RequireHttpsMetadata = false; // Set to true in production
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = jwtOptions.Issuer,
                ValidAudience = jwtOptions.Audience,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.SecretKey)),
                ClockSkew = TimeSpan.Zero
            };
        });

        // Add authorization
        services.AddAuthorization(options =>
        {
            // Add policies for different permission levels
            options.AddPolicy("AdminOnly", policy => 
                policy.RequireRole(AuthConstants.Roles.Admin));
                
            options.AddPolicy("AdminOrEmployee", policy => 
                policy.RequireRole(AuthConstants.Roles.Admin, AuthConstants.Roles.Employee));
                
            options.AddPolicy("AuthenticatedUser", policy => 
                policy.RequireAuthenticatedUser());

            // Permission-based policies
            options.AddPolicy("CanViewUsers", policy =>
                policy.RequireClaim(AuthConstants.ClaimTypes.Permission, AuthConstants.Permissions.Users.View));
                
            options.AddPolicy("CanManageUsers", policy =>
                policy.RequireClaim(AuthConstants.ClaimTypes.Permission, AuthConstants.Permissions.Users.Create, AuthConstants.Permissions.Users.Update, AuthConstants.Permissions.Users.Delete));
        });

        return services;
    }
}
