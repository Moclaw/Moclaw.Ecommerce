var builder = DistributedApplication.CreateBuilder(args);

var postgres = builder.AddPostgres("postgres").AddDatabase("mydb");

var userService = builder.AddProject<Projects.Ecom_Users_API>("User")
    .WithReference(postgres);

builder.AddProject<Projects.EcomCore_API>("Core")
    .WithReference(postgres)
    .WithReference(userService);

builder.Build().Run();
