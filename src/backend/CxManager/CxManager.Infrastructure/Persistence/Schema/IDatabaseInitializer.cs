namespace CxManager.Infrastructure.Persistence.Schema;

public interface IDatabaseInitializer
{
    Task InitializeAsync(CancellationToken cancellationToken = default);
}
