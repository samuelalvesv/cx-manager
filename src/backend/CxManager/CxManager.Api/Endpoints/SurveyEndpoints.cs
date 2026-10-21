using CxManager.Api.Filters;
using CxManager.Application.UseCases.Surveys.Commands.CreateSurvey;
using CxManager.Application.UseCases.Surveys.Commands.DeleteSurvey;
using CxManager.Application.UseCases.Surveys.Commands.UpdateSurvey;
using CxManager.Application.UseCases.Surveys.Queries;
using CxManager.Application.UseCases.Surveys.Queries.GetSurveyById;
using CxManager.Contract.Requests.Surveys;
using CxManager.Contract.Responses.Surveys;
using Microsoft.AspNetCore.Http.HttpResults;

namespace CxManager.Api.Endpoints;

public static class SurveyEndpoints
{
    public static RouteGroupBuilder MapSurveyEndpoints(this RouteGroupBuilder group)
    {
        group.MapGet(
            pattern: "/",
            handler: async Task<Ok<IReadOnlyList<SurveyResponse>>> (
                IGetSurveysUseCase useCase,
                CancellationToken cancellationToken) =>
            {
                IReadOnlyList<SurveyResponse> surveys = await useCase.ExecuteAsync(cancellationToken: cancellationToken);
                return TypedResults.Ok(value: surveys);
            })
            .WithName("GetSurveys")
            .WithSummary("Lista todas as pesquisas ativas da empresa.");

        group.MapGet(
            pattern: "/{id:long}",
            handler: async Task<Ok<SurveyResponse>> (
                [AsParameters] GetSurveyByIdRequest request,
                IGetSurveyByIdUseCase useCase,
                CancellationToken cancellationToken) =>
            {
                SurveyResponse survey = await useCase.ExecuteAsync(
                    request: request,
                    cancellationToken: cancellationToken);

                return TypedResults.Ok(value: survey);
            })
            .WithName("GetSurveyById")
            .WithSummary("Retorna os detalhes de uma pesquisa ativa da empresa.")
            .WithValidation<GetSurveyByIdRequest>();

        group.MapPost(
            pattern: "/",
            handler: async Task<Created<SurveyResponse>> (
                CreateSurveyRequest request,
                ICreateSurveyUseCase useCase,
                CancellationToken cancellationToken) =>
            {
                SurveyResponse survey = await useCase.ExecuteAsync(
                    request: request,
                    cancellationToken: cancellationToken);

                return TypedResults.Created(
                    uri: $"/api/surveys/{survey.Id}",
                    value: survey);
            })
            .WithName("CreateSurvey")
            .WithSummary("Cadastra uma nova pesquisa para a empresa ativa.")
            .WithValidation<CreateSurveyRequest>();

        group.MapPut(
            pattern: "/{id:long}",
            handler: async Task<Ok<SurveyResponse>> (
                [AsParameters] GetSurveyByIdRequest route,
                UpdateSurveyRequest request,
                IUpdateSurveyUseCase useCase,
                CancellationToken cancellationToken) =>
            {
                UpdateSurveyRequest command = request with { Id = route.Id };
                SurveyResponse survey = await useCase.ExecuteAsync(
                    request: command,
                    cancellationToken: cancellationToken);

                return TypedResults.Ok(value: survey);
            })
            .WithName("UpdateSurvey")
            .WithSummary("Atualiza uma pesquisa ativa da empresa.")
            .WithValidation<GetSurveyByIdRequest>()
            .WithValidation<UpdateSurveyRequest>();

        group.MapDelete(
            pattern: "/{id:long}",
            handler: async Task<NoContent> (
                [AsParameters] DeleteSurveyRequest request,
                IDeleteSurveyUseCase useCase,
                CancellationToken cancellationToken) =>
            {
                await useCase.ExecuteAsync(
                    request: request,
                    cancellationToken: cancellationToken);

                return TypedResults.NoContent();
            })
            .WithName("DeleteSurvey")
            .WithSummary("Desativa/exclui logicamente uma pesquisa da empresa.")
            .WithValidation<DeleteSurveyRequest>();

        return group;
    }
}
