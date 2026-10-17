IDistributedApplicationBuilder builder = DistributedApplication.CreateBuilder(args);

builder.AddProject<Projects.CxManager_Api>("cxmanager-api");

builder.Build().Run();
