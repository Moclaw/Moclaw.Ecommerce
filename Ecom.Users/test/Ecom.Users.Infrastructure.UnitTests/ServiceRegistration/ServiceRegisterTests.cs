using Autofac;
using Ecom.Users.Infrastructure;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Moq;

namespace Ecom.Users.Infrastructure.UnitTests.ServiceRegistration;

public class ServiceRegisterTests
{
    private readonly IServiceCollection _services;
    private readonly IConfiguration _configuration;
    private readonly ContainerBuilder _containerBuilder;

    public ServiceRegisterTests()
    {
        _services = new ServiceCollection();

        var configurationBuilder = new ConfigurationBuilder();
        configurationBuilder.AddInMemoryCollection(
            new Dictionary<string, string?>
            {
                ["ConnectionStrings:DefaultConnection"] =
                    "Host=localhost;Database=test;Username=test;Password=test"
            }
        );
        _configuration = configurationBuilder.Build();

        _containerBuilder = new ContainerBuilder();
    }

    [Fact]
    public void AddInfrastructureServices_ShouldRegisterDbContext()
    {
        // Act
        _services.AddInfrastructureServices(_configuration);

        // Assert
        var serviceProvider = _services.BuildServiceProvider();

        // Check if ApplicationDbContext is registered
        var dbContextDescriptor = _services.FirstOrDefault(
            d => d.ServiceType.Name.Contains("ApplicationDbContext")
        );
        dbContextDescriptor.Should().NotBeNull();
    }

    [Fact]
    public void AddInfrastructureServices_ShouldRegisterRepositories()
    {
        // Act
        _services.AddInfrastructureServices(_configuration);

        // Assert
        // Check if CommandRepository is registered with key
        var commandRepoDescriptor = _services.FirstOrDefault(
            d => d.ServiceType.Name.Contains("ICommandRepository")
        );
        commandRepoDescriptor.Should().NotBeNull();

        // Check if QueryRepository is registered with key
        var queryRepoDescriptor = _services.FirstOrDefault(
            d => d.ServiceType.Name.Contains("IQueryRepository")
        );
        queryRepoDescriptor.Should().NotBeNull();
    }

    [Fact]
    public void AddInfrastructureServices_WithNullConfiguration_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        var action = () => _services.AddInfrastructureServices(null!);
        action.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void AddInfrastructureServices_ContainerBuilder_ShouldRegisterServices()
    {
        // Act
        _containerBuilder.AddInfrastructureServices();
        var container = _containerBuilder.Build();

        // Assert
        container.Should().NotBeNull();

        // The container should be built successfully without exceptions
        // Additional specific service checks would require more complex setup
    }

    [Fact]
    public void AddInfrastructureServices_ShouldReturnServiceCollection()
    {
        // Act
        var result = _services.AddInfrastructureServices(_configuration);

        // Assert
        result.Should().BeSameAs(_services);
    }

    [Fact]
    public void AddInfrastructureServices_ShouldReturnContainerBuilder()
    {
        // Act
        var result = _containerBuilder.AddInfrastructureServices();

        // Assert
        result.Should().BeSameAs(_containerBuilder);
    }

    [Fact]
    public void AddInfrastructureServices_ShouldHandleMultipleCalls()
    {
        // Act
        _services.AddInfrastructureServices(_configuration);
        _services.AddInfrastructureServices(_configuration);

        // Assert
        // Should not throw exception on multiple calls
        var serviceProvider = _services.BuildServiceProvider();
        serviceProvider.Should().NotBeNull();
    }
}
