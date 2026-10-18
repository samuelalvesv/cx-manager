using CxManager.Domain.Entities;

namespace CxManager.Domain.Interfaces;

public interface ICompanyRepository
{
    Task<IReadOnlyList<Company>> GetAllActiveAsync(CancellationToken cancellationToken = default);
    Task<Company?> GetByIdAsync(long id, CancellationToken cancellationToken = default);
    Task<long> CreateAsync(Company company, CancellationToken cancellationToken = default);
    Task<bool> ExistsActiveAsync(long id, CancellationToken cancellationToken = default);
}
