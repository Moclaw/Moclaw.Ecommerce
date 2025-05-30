using Host;
using Host.Services;
using MinimalAPI;
using Ecom.Users.Application;
using Ecom.Users.Infrastructure;
using Ecom.Users.API.Middleware;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

var appName = builder.Environment.ApplicationName;
var configuration = builder.Configuration;

// Configure Serilog
builder.AddSerilog(configuration, appName);

// Register other services
builder
    .Services.AddCorsServices(configuration)
    .AddMinimalApi(
        typeof(Program).Assembly,
        typeof(Ecom.Users.Application.ServiceCollectionExtensions).Assembly,
        typeof(Ecom.Users.Infrastructure.ServiceCollectionExtensions).Assembly
    )
    .AddGlobalExceptionHandling(appName)
    .AddHealthCheck(configuration)
    // Register Infrastructure and Application services
    .AddInfrastructureServices(configuration)
    .AddApplicationServices()
    // Register OpenAPI/Swagger
    .AddOpenApi();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

// Configure CORS
app.UseCorsServices(configuration);

// Configure custom exception handling middleware
app.UseMiddleware<ExceptionHandlingMiddleware>();

// Configure Global Exception Handling
app.UseGlobalExceptionHandling();

// Configure ARM Elastic
app.UseElasticApm(configuration);

app.UseRouting();

// Add Authentication and Authorization middleware
app.UseAuthentication();
app.UseAuthorization();

// Configure Health Check
app.UseHealthChecks(configuration);

// Map all endpoints from the assembly
app.MapMinimalEndpoints(null, typeof(Program).Assembly);

await app.RunAsync();
