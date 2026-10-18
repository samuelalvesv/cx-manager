using CxManager.Api.Endpoints;
using CxManager.Infrastructure.Persistence.Schema;
using CxManager.Infrastructure.Persistence.Seed;

namespace CxManager.Api.Extensions;

public static class AppExtensions
{
    public static WebApplication UseApiPipeline(this WebApplication app)
    {
        app.UseExceptionHandler();

        if (app.Environment.IsDevelopment())
        {
            app.MapOpenApi();
        }

        app.UseHttpsRedirection();

        return app;
    }

    public static WebApplication MapApiEndpoints(this WebApplication app)
    {
        RouteGroupBuilder apiGroup = app.MapGroup(prefix: "/api");

        apiGroup.MapGroup(prefix: "/companies")
            .MapCompanyEndpoints();

        return app;
    }

    public static async Task<WebApplication> InitializeDatabaseAsync(this WebApplication app)
    {
        using IServiceScope scope = app.Services.CreateScope();

        IDatabaseInitializer databaseInitializer = scope.ServiceProvider.GetRequiredService<IDatabaseInitializer>();
        await databaseInitializer.InitializeAsync();

        ISeedImporter seedImporter = scope.ServiceProvider.GetRequiredService<ISeedImporter>();
        await seedImporter.ImportAsync();

        return app;
    }
}
