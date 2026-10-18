using System.Data.Common;

namespace CxManager.Infrastructure.Persistence;

public interface IDbConnectionFactory
{
    Task<DbConnection> CreateConnectionAsync(CancellationToken cancellationToken = default);
}
