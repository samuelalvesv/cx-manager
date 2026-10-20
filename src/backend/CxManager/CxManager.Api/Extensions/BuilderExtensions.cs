using System.Text.Json.Serialization;
using CxManager.Api.Context;
using CxManager.Api.ExceptionHandling;
using CxManager.Api.Serialization;
using CxManager.Application.Interfaces;
using CxManager.Application.UseCases.Companies.Commands.CreateCompany;
using CxManager.Application.UseCases.Companies.Commands.DeleteCompany;
using CxManager.Application.UseCases.Companies.Commands.UpdateCompany;
using CxManager.Application.UseCases.Companies.Queries;
using CxManager.Application.UseCases.Companies.Queries.GetCompanyById;
using CxManager.Application.UseCases.Surveys.Commands.CreateSurvey;
using CxManager.Application.UseCases.Surveys.Commands.DeleteSurvey;
using CxManager.Application.UseCases.Surveys.Commands.UpdateSurvey;
using CxManager.Application.UseCases.Surveys.Queries;
using CxManager.Application.UseCases.Surveys.Queries.GetSurveyById;
using CxManager.Application.Validators.Companies;
using CxManager.Application.Validators.Surveys;
using CxManager.Contract.Requests.Companies;
using CxManager.Contract.Requests.Surveys;
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

        // Casos de uso de aplicação - Empresas
        builder.Services.AddScoped<IGetCompaniesUseCase, GetCompaniesUseCase>();
        builder.Services.AddScoped<IGetCompanyByIdUseCase, GetCompanyByIdUseCase>();
        builder.Services.AddScoped<ICreateCompanyUseCase, CreateCompanyUseCase>();
        builder.Services.AddScoped<IUpdateCompanyUseCase, UpdateCompanyUseCase>();
        builder.Services.AddScoped<IDeleteCompanyUseCase, DeleteCompanyUseCase>();

        // Casos de uso de aplicação - Pesquisas
        builder.Services.AddScoped<IGetSurveysUseCase, GetSurveysUseCase>();
        builder.Services.AddScoped<IGetSurveyByIdUseCase, GetSurveyByIdUseCase>();
        builder.Services.AddScoped<ICreateSurveyUseCase, CreateSurveyUseCase>();
        builder.Services.AddScoped<IUpdateSurveyUseCase, UpdateSurveyUseCase>();
        builder.Services.AddScoped<IDeleteSurveyUseCase, DeleteSurveyUseCase>();

        // Validadores
        builder.Services.AddScoped<FluentValidation.IValidator<CreateCompanyRequest>, CreateCompanyRequestValidator>();
        builder.Services.AddScoped<FluentValidation.IValidator<UpdateCompanyRequest>, UpdateCompanyRequestValidator>();
        builder.Services.AddScoped<FluentValidation.IValidator<CreateSurveyRequest>, CreateSurveyRequestValidator>();
        builder.Services.AddScoped<FluentValidation.IValidator<UpdateSurveyRequest>, UpdateSurveyRequestValidator>();

        return builder;
    }
}
