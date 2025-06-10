using Autofac;
using Autofac.Extensions.DependencyInjection;
using Ecom.Users.API.Middleware;
using Ecom.Users.Application;
using Ecom.Users.Domain.ValueObjects;
using Ecom.Users.Infrastructure;
using Ecom.Users.Infrastructure.Persistence.EfCore;
using Host;
using Host.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using MinimalAPI;
using MinimalAPI.OpenApi;
using MinimalAPI.SwaggerUI;
using Serilog;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

var appName = builder.Environment.ApplicationName;
var configuration = builder.Configuration;

// Configure Serilog
builder.AddSerilog(configuration, appName);

// Add health checks
builder.Services.AddHealthChecks();

// Configure JWT Authentication
var jwtOptions = configuration.GetSection("JwtOptions").Get<JwtOptions>();
builder
    .Services.AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtOptions?.Issuer,
            ValidAudience = jwtOptions?.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(jwtOptions?.Secret ?? "default_key_for_development_only")
            ),
            ClockSkew = TimeSpan.Zero,
        };
    });

// Register authorization
builder.Services.AddAuthorization();

// Add API Explorer services for Swagger
builder.Services.AddEndpointsApiExplorer();

var versioningOptions = new DefaultVersioningOptions
{
    BaseRouteTemplate = "api",
    DefaultVersion = 1,
    AssumeDefaultVersionWhenUnspecified = true,
    GenerateSwaggerDocs = true,
};

// Register other services
builder
    .Services.AddCorsServices(configuration)
    .AddMinimalApiWithSwaggerUI(
        title: "Ecom Users API",
        version: "v1",
        description: "User management and authentication API",
        versioningOptions: versioningOptions,
        assemblies:
        [
            typeof(Program).Assembly,
            typeof(Ecom.Users.Application.Register).Assembly,
            typeof(Ecom.Users.Infrastructure.Register).Assembly,
        ]
    )
    .AddAutofac()
    .AddGlobalExceptionHandling(appName)
    .AddHealthCheck(configuration)
    // Register Infrastructure and Application services
    .AddInfrastructureServices(configuration)
    .AddApplicationServices(configuration);

// Configure Autofac after all service registrations
builder.Host.UseServiceProviderFactory(new AutofacServiceProviderFactory());
builder.Host.ConfigureContainer<ContainerBuilder>(containerBuilder =>
{
    // Register application and infrastructure services with Autofac
    containerBuilder.AddApplicationServices().AddInfrastructureServices();
});
var app = builder.Build();

app.MapMinimalEndpoints(versioningOptions, typeof(Program).Assembly);

if (app.Environment.IsDevelopment())
{
    app.UseMinimalApiOpenApi();

    app.UseMinimalApiDocs(
        swaggerRoutePrefix: "docs",
        enableTryItOut: true,
        enableDeepLinking: true,
        enableFilter: true
    );
}

app.UseHttpsRedirection();

// Configure CORS
// app.UseCorsServices(configuration);

// Configure Global Exception Handling
app.UseGlobalExceptionHandling();

// Configure ARM Elastic (disable in Kubernetes to prevent connection issues)
if (!builder.Environment.IsProduction())
{
    app.UseElasticApm(configuration);
}

app.UseRouting();

// Configure Authentication and Authorization
app.UseAuthentication();
app.UseAuthorization();

// Configure Permission Middleware
app.UsePermissionMiddleware();

// Configure Health Check endpoint
app.MapHealthChecks("/health");

await app.RunAsync();
