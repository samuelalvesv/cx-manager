using CxManager.Domain.Interfaces;
using CxManager.Infrastructure.Persistence;
using CxManager.Infrastructure.Persistence.Repositories;
using CxManager.Infrastructure.Persistence.Schema;
using CxManager.Infrastructure.Persistence.Seed;
using Microsoft.Extensions.DependencyInjection;

namespace CxManager.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, string connectionString)
    {
        services.AddSingleton<IDbConnectionFactory>(implementationInstance: new SqliteConnectionFactory(connectionString: connectionString));
        services.AddSingleton<IDatabaseInitializer, DatabaseInitializer>();
        services.AddSingleton<ISeedImporter, SeedImporter>();
        services.AddScoped<ICompanyRepository, CompanyRepository>();
        services.AddScoped<ISurveyRepository, SurveyRepository>();
        services.AddScoped<IContactRepository, ContactRepository>();

        return services;
    }
}
