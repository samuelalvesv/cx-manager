using CxManager.Api.Extensions;
using CxManager.ServiceDefaults;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args: args);

builder.AddServiceDefaults();
builder.Services.AddOpenApi();
builder.AddDatabasePersistence();
builder.AddApiServices();

WebApplication app = builder.Build();

app.UseApiPipeline();
app.MapDefaultEndpoints();
app.MapApiEndpoints();

await app.InitializeDatabaseAsync();

app.Run();

public partial class Program
{
    protected Program()
    {
    }
}
