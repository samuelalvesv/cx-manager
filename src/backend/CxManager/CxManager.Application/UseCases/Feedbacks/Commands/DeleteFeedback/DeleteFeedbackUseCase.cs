using CxManager.Application.Interfaces;
using CxManager.Contract.Requests.Feedbacks;
using CxManager.Domain.Exceptions;
using CxManager.Domain.Interfaces;

namespace CxManager.Application.UseCases.Feedbacks.Commands.DeleteFeedback;

public sealed class DeleteFeedbackUseCase(
    IFeedbackRepository feedbackRepository,
    IAnalyticsCache analyticsCache,
    ICompanyContext companyContext) : IDeleteFeedbackUseCase
{
    public async Task ExecuteAsync(DeleteFeedbackRequest request, CancellationToken cancellationToken = default)
    {
        long companyId = companyContext.CompanyId;

        bool exists = await feedbackRepository.ExistsActiveAsync(
            id: request.Id,
            companyId: companyId,
            cancellationToken: cancellationToken);

        if (!exists)
            throw new EntityNotFoundException(message: "Resposta não encontrada para a empresa ativa.");

        await feedbackRepository.DeleteAsync(
            id: request.Id,
            companyId: companyId,
            deletedAt: DateTimeOffset.UtcNow,
            cancellationToken: cancellationToken);

        analyticsCache.InvalidateCompany(companyId: companyId);
    }
}
