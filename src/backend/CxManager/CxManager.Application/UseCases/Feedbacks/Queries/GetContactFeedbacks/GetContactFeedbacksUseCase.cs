using CxManager.Application.Interfaces;
using CxManager.Application.Mappers;
using CxManager.Application.Models;
using CxManager.Contract.Requests.Feedbacks;
using CxManager.Contract.Responses.Feedbacks;
using CxManager.Domain.Exceptions;
using CxManager.Domain.Interfaces;

namespace CxManager.Application.UseCases.Feedbacks.Queries.GetContactFeedbacks;

public sealed class GetContactFeedbacksUseCase(
    IFeedbackQueryService feedbackQueryService,
    IContactRepository contactRepository,
    ICompanyContext companyContext) : IGetContactFeedbacksUseCase
{
    public async Task<IReadOnlyList<FeedbackResponse>> ExecuteAsync(GetContactFeedbacksRequest request, CancellationToken cancellationToken = default)
    {
        long companyId = companyContext.CompanyId;

        bool contactExists = await contactRepository.ExistsActiveAsync(
            id: request.Id,
            companyId: companyId,
            cancellationToken: cancellationToken);

        if (!contactExists)
            throw new EntityNotFoundException(message: "Contato não encontrado para a empresa ativa.");

        IReadOnlyList<FeedbackReadModel> details = await feedbackQueryService.GetActiveByContactIdAsync(
            contactId: request.Id,
            companyId: companyId,
            cancellationToken: cancellationToken);

        return FeedbackMapper.ToResponseList(models: details);
    }
}
