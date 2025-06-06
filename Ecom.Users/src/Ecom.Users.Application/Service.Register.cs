using Autofac;
using Autofac.Extensions.DependencyInjection;
using Ecom.Users.Domain.ValueObjects;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Services.Autofac.Attributes;
using Services.Autofac.Extensions;

namespace Ecom.Users.Application
{
    public static partial class Register
    {
        public static IServiceCollection AddApplicationServices(
            this IServiceCollection services,
            IConfiguration configuration
        )
        {
            // Configuration options
            services.Configure<JwtOptions>(configuration.GetSection("JwtOptions"));
            services.Configure<GoogleAuthOptions>(configuration.GetSection("GoogleAuthOptions"));
            services.Configure<FacebookAuthOptions>(
                configuration.GetSection("FacebookAuthOptions")
            );

            return services;
        }
        public static ContainerBuilder AddApplicationServices(this ContainerBuilder builder)
        {
            // Register services from this assembly using attribute-based registration
            builder.RegisterServiceAssemblies(true, false, typeof(Register).Assembly);

            return builder;
        }

    }
}
