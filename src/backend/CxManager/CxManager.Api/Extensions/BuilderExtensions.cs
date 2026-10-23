using System.Text.Json.Serialization;
using CxManager.Api.Context;
using CxManager.Api.ExceptionHandling;
using CxManager.Api.Serialization;
using CxManager.Application.Interfaces;
using CxManager.Application.UseCases.Analytics.Queries.GetAnalyticsSummary;
using CxManager.Application.UseCases.Analytics.Queries.GetNpsByMonth;
using CxManager.Application.UseCases.Analytics.Queries.GetNpsBySegment;
using CxManager.Application.UseCases.Companies.Commands.CreateCompany;
using CxManager.Application.UseCases.Companies.Commands.DeleteCompany;
using CxManager.Application.UseCases.Companies.Commands.UpdateCompany;
using CxManager.Application.UseCases.Companies.Queries;
using CxManager.Application.UseCases.Companies.Queries.GetCompanyById;
using CxManager.Application.UseCases.Contacts.Commands.CreateContact;
using CxManager.Application.UseCases.Contacts.Commands.DeleteContact;
using CxManager.Application.UseCases.Contacts.Commands.UpdateContact;
using CxManager.Application.UseCases.Contacts.Queries;
using CxManager.Application.UseCases.Contacts.Queries.GetContactById;
using CxManager.Application.UseCases.Feedbacks.Commands.CreateFeedback;
using CxManager.Application.UseCases.Feedbacks.Commands.DeleteFeedback;
using CxManager.Application.UseCases.Feedbacks.Commands.UpdateFeedback;
using CxManager.Application.UseCases.Feedbacks.Queries.GetContactFeedbacks;
using CxManager.Application.UseCases.Feedbacks.Queries.GetFeedbackById;
using CxManager.Application.UseCases.Surveys.Commands.CreateSurvey;
using CxManager.Application.UseCases.Surveys.Commands.DeleteSurvey;
using CxManager.Application.UseCases.Surveys.Commands.UpdateSurvey;
using CxManager.Application.UseCases.Surveys.Queries;
using CxManager.Application.UseCases.Surveys.Queries.GetSurveyById;
using CxManager.Application.Validators.Analytics;
using CxManager.Application.Validators.Companies;
using CxManager.Application.Validators.Contacts;
using CxManager.Application.Validators.Feedbacks;
using CxManager.Application.Validators.Surveys;
using CxManager.Contract.Requests.Analytics;
using CxManager.Contract.Requests.Companies;
using CxManager.Contract.Requests.Contacts;
using CxManager.Contract.Requests.Feedbacks;
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
            options.SerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.Never;
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

        // Casos de uso de aplicação - Contatos
        builder.Services.AddScoped<IGetContactsUseCase, GetContactsUseCase>();
        builder.Services.AddScoped<IGetContactByIdUseCase, GetContactByIdUseCase>();
        builder.Services.AddScoped<ICreateContactUseCase, CreateContactUseCase>();
        builder.Services.AddScoped<IUpdateContactUseCase, UpdateContactUseCase>();
        builder.Services.AddScoped<IDeleteContactUseCase, DeleteContactUseCase>();

        // Casos de uso de aplicação - Feedbacks
        builder.Services.AddScoped<ICreateFeedbackUseCase, CreateFeedbackUseCase>();
        builder.Services.AddScoped<IUpdateFeedbackUseCase, UpdateFeedbackUseCase>();
        builder.Services.AddScoped<IDeleteFeedbackUseCase, DeleteFeedbackUseCase>();
        builder.Services.AddScoped<IGetFeedbackByIdUseCase, GetFeedbackByIdUseCase>();
        builder.Services.AddScoped<IGetContactFeedbacksUseCase, GetContactFeedbacksUseCase>();

        // Casos de uso de aplicação - Analytics
        builder.Services.AddScoped<IGetAnalyticsSummaryUseCase, GetAnalyticsSummaryUseCase>();
        builder.Services.AddScoped<IGetNpsByMonthUseCase, GetNpsByMonthUseCase>();
        builder.Services.AddScoped<IGetNpsBySegmentUseCase, GetNpsBySegmentUseCase>();

        // Validadores
        builder.Services.AddScoped<FluentValidation.IValidator<CreateCompanyRequest>, CreateCompanyRequestValidator>();
        builder.Services.AddScoped<FluentValidation.IValidator<UpdateCompanyRequest>, UpdateCompanyRequestValidator>();
        builder.Services.AddScoped<FluentValidation.IValidator<GetCompanyByIdRequest>, GetCompanyByIdRequestValidator>();
        builder.Services.AddScoped<FluentValidation.IValidator<DeleteCompanyRequest>, DeleteCompanyRequestValidator>();
        builder.Services.AddScoped<FluentValidation.IValidator<CreateSurveyRequest>, CreateSurveyRequestValidator>();
        builder.Services.AddScoped<FluentValidation.IValidator<UpdateSurveyRequest>, UpdateSurveyRequestValidator>();
        builder.Services.AddScoped<FluentValidation.IValidator<GetSurveyByIdRequest>, GetSurveyByIdRequestValidator>();
        builder.Services.AddScoped<FluentValidation.IValidator<DeleteSurveyRequest>, DeleteSurveyRequestValidator>();
        builder.Services.AddScoped<FluentValidation.IValidator<CreateContactRequest>, CreateContactRequestValidator>();
        builder.Services.AddScoped<FluentValidation.IValidator<UpdateContactRequest>, UpdateContactRequestValidator>();
        builder.Services.AddScoped<FluentValidation.IValidator<GetContactsRequest>, GetContactsRequestValidator>();
        builder.Services.AddScoped<FluentValidation.IValidator<GetContactByIdRequest>, GetContactByIdRequestValidator>();
        builder.Services.AddScoped<FluentValidation.IValidator<DeleteContactRequest>, DeleteContactRequestValidator>();
        builder.Services.AddScoped<FluentValidation.IValidator<CreateFeedbackRequest>, CreateFeedbackRequestValidator>();
        builder.Services.AddScoped<FluentValidation.IValidator<UpdateFeedbackRequest>, UpdateFeedbackRequestValidator>();
        builder.Services.AddScoped<FluentValidation.IValidator<GetFeedbackByIdRequest>, GetFeedbackByIdRequestValidator>();
        builder.Services.AddScoped<FluentValidation.IValidator<GetContactFeedbacksRequest>, GetContactFeedbacksRequestValidator>();
        builder.Services.AddScoped<FluentValidation.IValidator<DeleteFeedbackRequest>, DeleteFeedbackRequestValidator>();
        builder.Services.AddScoped<FluentValidation.IValidator<AnalyticsFilterRequest>, AnalyticsFilterRequestValidator>();

        return builder;
    }
}
