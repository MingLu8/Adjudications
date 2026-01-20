var builder = DistributedApplication.CreateBuilder(args);

builder.AddProject<Projects.FormularyApi>("formularyapi");

builder.Build().Run();
