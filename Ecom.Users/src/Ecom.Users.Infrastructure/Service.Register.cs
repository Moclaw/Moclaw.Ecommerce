using DotnetCap;
using EfCore.IRepositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Ecom.Users.Domain.Constants;
using Ecom.Users.Infrastructure.Persistence.EfCore;
using Ecom.Users.Infrastructure.Repositories;

namespace Ecom.Users.Infrastructure
{
    public static partial class Register
    {
        public static IServiceCollection AddInfrastructureServices(
            this IServiceCollection services,
            IConfiguration configuration
        )
        {
            ArgumentNullException.ThrowIfNull(configuration);
            
            // Register DbContext
            services.AddDbContext<ApplicationDbContext>(options =>
            {
                options.UseNpgsql(configuration.GetConnectionString("DefaultConnection"))
                    .EnableSensitiveDataLogging()
                    .LogTo(Console.WriteLine, LogLevel.Information);
            });

            services.AddScoped<ApplicationDbContext>();

            //services.AddDotnetCap(configuration).AddRabbitMq(configuration);

            services.AddKeyedScoped<ICommandRepository, CommandDefaultRepository>(ServiceKeys.CommandRepository);

            services.TryAddKeyedScoped(
                typeof(IQueryRepository<,>),
                ServiceKeys.QueryRepository,
                typeof(QueryDefaultRepository<,>)
            );

            return services;
        }
    }
}
