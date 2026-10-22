using CxManager.Application.Interfaces;
using CxManager.Application.Mappers;
using CxManager.Application.Models;
using CxManager.Contract.Requests.Feedbacks;
using CxManager.Contract.Responses.Feedbacks;
using CxManager.Domain.Exceptions;

namespace CxManager.Application.UseCases.Feedbacks.Queries.GetFeedbackById;

public sealed class GetFeedbackByIdUseCase(
    IFeedbackQueryService feedbackQueryService,
    ICompanyContext companyContext) : IGetFeedbackByIdUseCase
{
    public async Task<FeedbackResponse> ExecuteAsync(GetFeedbackByIdRequest request, CancellationToken cancellationToken = default)
    {
        long companyId = companyContext.CompanyId;

        FeedbackReadModel? detail = await feedbackQueryService.GetDetailByIdAsync(
            id: request.Id,
            companyId: companyId,
            cancellationToken: cancellationToken);

        if (detail is null)
            throw new EntityNotFoundException(message: "Resposta não encontrada para a empresa ativa.");

        return FeedbackMapper.ToResponse(model: detail);
    }
}
