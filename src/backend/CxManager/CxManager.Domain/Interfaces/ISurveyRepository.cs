using CxManager.Domain.Entities;

namespace CxManager.Domain.Interfaces;

public interface ISurveyRepository
{
    Task<IReadOnlyList<Survey>> GetAllActiveByCompanyAsync(long companyId, CancellationToken cancellationToken = default);
    Task<Survey?> GetByIdAsync(long id, long companyId, CancellationToken cancellationToken = default);
    Task<IReadOnlyDictionary<long, int>> GetActiveResponsesCountsByCompanyAsync(long companyId, CancellationToken cancellationToken = default);
    Task<int> GetActiveResponsesCountAsync(long surveyId, long companyId, CancellationToken cancellationToken = default);
    Task<long> CreateAsync(Survey survey, CancellationToken cancellationToken = default);
    Task UpdateAsync(Survey survey, CancellationToken cancellationToken = default);
    Task DeleteAsync(long id, long companyId, DateTimeOffset deletedAt, CancellationToken cancellationToken = default);
    Task<bool> ExistsActiveAsync(long id, long companyId, CancellationToken cancellationToken = default);
}
