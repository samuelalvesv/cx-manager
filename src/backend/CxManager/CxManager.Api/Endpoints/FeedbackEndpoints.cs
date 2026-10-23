using CxManager.Api.Filters;
using CxManager.Application.UseCases.Feedbacks.Commands.CreateFeedback;
using CxManager.Application.UseCases.Feedbacks.Commands.DeleteFeedback;
using CxManager.Application.UseCases.Feedbacks.Commands.UpdateFeedback;
using CxManager.Application.UseCases.Feedbacks.Queries.ExportFeedbacks;
using CxManager.Application.UseCases.Feedbacks.Queries.GetFeedbackById;
using CxManager.Contract.Requests.Analytics;
using CxManager.Contract.Requests.Feedbacks;
using CxManager.Contract.Responses.Feedbacks;
using Microsoft.AspNetCore.Http.HttpResults;

namespace CxManager.Api.Endpoints;

public static class FeedbackEndpoints
{
    public static RouteGroupBuilder MapFeedbackEndpoints(this RouteGroupBuilder group)
    {
        group.MapPost(
            pattern: "/",
            handler: async Task<Created<FeedbackResponse>> (
                CreateFeedbackRequest request,
                ICreateFeedbackUseCase useCase,
                CancellationToken cancellationToken) =>
            {
                FeedbackResponse response = await useCase.ExecuteAsync(
                    request: request,
                    cancellationToken: cancellationToken);

                return TypedResults.Created(
                    uri: $"/api/responses/{response.Id}",
                    value: response);
            })
            .WithName("CreateFeedback")
            .WithSummary("Registra uma nova resposta de pesquisa para um contato.")
            .WithValidation<CreateFeedbackRequest>();

        group.MapGet(
            pattern: "/export",
            handler: async Task<FileContentHttpResult> (
                [AsParameters] AnalyticsFilterRequest request,
                IExportFeedbacksUseCase useCase,
                CancellationToken cancellationToken) =>
            {
                byte[] bytes = await useCase.ExecuteAsync(
                    request: request,
                    cancellationToken: cancellationToken);

                return TypedResults.File(
                    fileContents: bytes,
                    contentType: "text/csv; charset=utf-8",
                    fileDownloadName: "respostas.csv");
            })
            .WithName("ExportFeedbacks")
            .WithSummary("Exporta as respostas ativas em CSV compatível com RFC 4180.")
            .WithValidation<AnalyticsFilterRequest>();

        group.MapGet(
            pattern: "/{id:long}",
            handler: async Task<Ok<FeedbackResponse>> (
                [AsParameters] GetFeedbackByIdRequest request,
                IGetFeedbackByIdUseCase useCase,
                CancellationToken cancellationToken) =>
            {
                FeedbackResponse response = await useCase.ExecuteAsync(
                    request: request,
                    cancellationToken: cancellationToken);

                return TypedResults.Ok(value: response);
            })
            .WithName("GetFeedbackById")
            .WithSummary("Retorna os detalhes de uma resposta ativa da empresa.")
            .WithValidation<GetFeedbackByIdRequest>();

        group.MapPut(
            pattern: "/{id:long}",
            handler: async Task<Ok<FeedbackResponse>> (
                [AsParameters] GetFeedbackByIdRequest route,
                UpdateFeedbackRequest request,
                IUpdateFeedbackUseCase useCase,
                CancellationToken cancellationToken) =>
            {
                UpdateFeedbackRequest command = request with { Id = route.Id };
                FeedbackResponse response = await useCase.ExecuteAsync(
                    request: command,
                    cancellationToken: cancellationToken);

                return TypedResults.Ok(value: response);
            })
            .WithName("UpdateFeedback")
            .WithSummary("Atualiza a nota e/ou comentário de uma resposta ativa.")
            .WithValidation<GetFeedbackByIdRequest>()
            .WithValidation<UpdateFeedbackRequest>();

        group.MapDelete(
            pattern: "/{id:long}",
            handler: async Task<NoContent> (
                [AsParameters] DeleteFeedbackRequest request,
                IDeleteFeedbackUseCase useCase,
                CancellationToken cancellationToken) =>
            {
                await useCase.ExecuteAsync(
                    request: request,
                    cancellationToken: cancellationToken);

                return TypedResults.NoContent();
            })
            .WithName("DeleteFeedback")
            .WithSummary("Exclui logicamente uma resposta.")
            .WithValidation<DeleteFeedbackRequest>();

        return group;
    }
}
