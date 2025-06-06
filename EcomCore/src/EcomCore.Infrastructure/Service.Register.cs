using Autofac;
using EcomCore.Domain.Constants;
using EcomCore.Infrastructure.Persistence.EfCore;
using EcomCore.Infrastructure.Repositories;
using EfCore.IRepositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Services.Autofac.Extensions;

namespace EcomCore.Infrastructure
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

            // Add infrastructure services here
            return services;
        }

        public static ContainerBuilder AddInfrastructureServices(this ContainerBuilder builder)
        {
            // Register services from this assembly using attribute-based registration
            builder.RegisterServiceAssemblies(true, false, typeof(Register).Assembly);

            return builder;
        }
    }
}