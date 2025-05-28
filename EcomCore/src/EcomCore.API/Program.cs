using EcomCore.Application;
using EcomCore.Infrastructure;
using Host;
using Host.Services;
using MinimalAPI;
using Microsoft.EntityFrameworkCore;
using MinimalAPI.OpenApi;
using MinimalAPI.SwaggerUI;

var builder = WebApplication.CreateBuilder(args);

var appName = builder.Environment.ApplicationName;
var configuration = builder.Configuration;

// Configure Serilog
builder.AddSerilog(configuration, appName);

// Register other services
builder
    .Services.AddCorsServices(configuration)
    .AddMinimalApiWithSwaggerUI(
        title: "EcomCore API",
        version: "v1",
        description: "EcomCore API for e-commerce applications",
        endpointAssemblies: [
            typeof(Program).Assembly,
            typeof(EcomCore.Application.Register).Assembly,
            typeof(EcomCore.Infrastructure.Register).Assembly,
            ]
    )
    .AddGlobalExceptionHandling(appName)
    .AddHealthCheck(configuration)
    // Register Infrastructure and Application services
    .AddInfrastructureServices(configuration)
    .AddApplicationServices(configuration);

var app = builder.Build();

// Auto-migrate database
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<EcomCore.Infrastructure.Persistence.EfCore.ApplicationDbContext>();
    db.Database.Migrate();
}

if (app.Environment.IsDevelopment())
{
    app.UseMinimalApiSwaggerUI(
       routePrefix: "api-docs"
    );
}

app.UseHttpsRedirection();

// Configure CORS
app.UseCorsServices(configuration);

// Configure Global Exception Handling
app.UseGlobalExceptionHandling();

// Configure ARM Elastic
app.UseElasticApm(configuration);

app.UseRouting();

// Configure Health Check
//app.UseHealthChecks(configuration);

// Map all endpoints from the assembly
app.MapMinimalEndpoints(typeof(Program).Assembly);

await app.RunAsync();
