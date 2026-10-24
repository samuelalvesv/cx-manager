using CxManager.Domain.Entities;

namespace CxManager.Domain.Interfaces;

public interface IContactRepository
{
    Task<(IReadOnlyList<Contact> Items, int Total)> GetPagedByCompanyAsync(
        long companyId,
        int page,
        int pageSize,
        string? search = null,
        string? sortBy = null,
        string? sortDir = null,
        CancellationToken cancellationToken = default);

    Task<Contact?> GetByIdAsync(
        long id,
        long companyId,
        CancellationToken cancellationToken = default);

    Task<long> CreateAsync(
        Contact contact,
        CancellationToken cancellationToken = default);

    Task UpdateAsync(
        Contact contact,
        CancellationToken cancellationToken = default);

    Task DeleteAsync(
        long id,
        long companyId,
        DateTimeOffset deletedAt,
        CancellationToken cancellationToken = default);

    Task<bool> ExistsActiveAsync(
        long id,
        long companyId,
        CancellationToken cancellationToken = default);

    Task<int> GetActiveCountByCompanyAsync(
        long companyId,
        CancellationToken cancellationToken = default);

    Task<bool> ExistsActiveEmailAsync(
        long companyId,
        string email,
        long? excludeContactId = null,
        CancellationToken cancellationToken = default);
}
