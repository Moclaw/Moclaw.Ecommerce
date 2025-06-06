var builder = DistributedApplication.CreateBuilder(args);

var postgres = builder.AddPostgres("Ecom")
    .WithImage("postgres");

var userSql = postgres.AddDatabase("Ecom.User");

var coreSql = postgres.AddDatabase("Ecom.Core");

var userService = builder.AddProject<Projects.Ecom_Users_API>("User").
    WithReference(userSql);

builder.AddProject<Projects.EcomCore_API>("Core")
    .WithReference(coreSql)
    .WithReference(userService);

builder.Build().Run();
