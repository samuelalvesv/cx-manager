using System.Text.Json.Serialization;
using CxManager.Api.Context;
using CxManager.Api.ExceptionHandling;
using CxManager.Api.Serialization;
using CxManager.Application.Interfaces;
using CxManager.Application.UseCases.Companies.Queries;
using CxManager.Infrastructure;
using Microsoft.AspNetCore.Http.Json;

namespace CxManager.Api.Extensions;

public static class BuilderExtensions
{
    public static WebApplicationBuilder AddDatabasePersistence(this WebApplicationBuilder builder)
    {
        string connectionString = builder.Configuration.GetConnectionString(name: "DefaultConnection")
            ?? "Data Source=cxmanager.db;Cache=Shared";

        builder.Services.AddInfrastructure(connectionString: connectionString);

        return builder;
    }

    public static WebApplicationBuilder AddApiServices(this WebApplicationBuilder builder)
    {
        builder.Services.Configure<JsonOptions>(configureOptions: options =>
        {
            options.SerializerOptions.TypeInfoResolverChain.Insert(index: 0, item: AppJsonSerializerContext.Default);
            options.SerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
            options.SerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
        });

        builder.Services.AddHttpContextAccessor();
        builder.Services.AddProblemDetails();
        builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

        // Contexto multi-empresa
        builder.Services.AddScoped<ICompanyContext, HttpCompanyContext>();

        // Casos de uso de aplicação
        builder.Services.AddScoped<IGetCompaniesUseCase, GetCompaniesUseCase>();

        return builder;
    }
}
