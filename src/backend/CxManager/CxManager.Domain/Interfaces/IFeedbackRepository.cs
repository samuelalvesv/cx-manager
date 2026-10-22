using CxManager.Domain.Entities;

namespace CxManager.Domain.Interfaces;

public interface IFeedbackRepository
{
    Task<long> CreateAsync(Feedback feedback, CancellationToken cancellationToken = default);
    Task<Feedback?> GetByIdAsync(long id, long companyId, CancellationToken cancellationToken = default);
    Task UpdateAsync(Feedback feedback, CancellationToken cancellationToken = default);
    Task DeleteAsync(long id, long companyId, DateTimeOffset deletedAt, CancellationToken cancellationToken = default);
    Task<bool> ExistsActiveAsync(long id, long companyId, CancellationToken cancellationToken = default);
}
