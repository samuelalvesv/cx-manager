using CxManager.Application.Models;

namespace CxManager.Application.Interfaces;

public interface IFeedbackQueryService
{
    Task<FeedbackReadModel?> GetDetailByIdAsync(long id, long companyId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<FeedbackReadModel>> GetActiveByContactIdAsync(long contactId, long companyId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<FeedbackExportModel>> GetForExportAsync(
        long companyId,
        DateOnly? from,
        DateOnly? to,
        long? surveyId,
        CancellationToken cancellationToken = default);
}
