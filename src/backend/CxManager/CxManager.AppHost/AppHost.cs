IDistributedApplicationBuilder builder = DistributedApplication.CreateBuilder(args: args);

IResourceBuilder<ProjectResource> api = builder.AddProject<Projects.CxManager_Api>(name: "cxmanager-api");

IResourceBuilder<NodeAppResource> frontend = builder.AddNpmApp(name: "frontend", workingDirectory: "../../../frontend", scriptName: "dev")
    .WithReference(source: api)
    .WithHttpEndpoint(port: 5173, env: "PORT");

if (builder.ExecutionContext.IsPublishMode)
{
    frontend.PublishAsDockerFile();
}

builder.Build().Run();
