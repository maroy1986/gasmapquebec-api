var builder = DistributedApplication.CreateBuilder(args);

var postgres = builder.AddPostgres("postgres")
    .WithDataVolume()
    .WithPgAdmin();

var gasmapdb = postgres.AddDatabase("gasmapdb");

builder.AddProject<Projects.API>("api")
    .WithReference(gasmapdb)
    .WaitFor(gasmapdb);

builder.Build().Run();
