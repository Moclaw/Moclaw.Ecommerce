using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Services.AWS.S3;

namespace EcomCore.Application
{
    public static partial class Register
    {
        public static IServiceCollection AddApplicationServices(
            this IServiceCollection services,
            IConfiguration configuration
        )
        {
            ArgumentNullException.ThrowIfNull(configuration);
            services.AddS3Services(configuration);

            return services;
        }
    }
}
