namespace CxManager.Infrastructure.Persistence.Seed;

public interface ISeedImporter
{
    Task ImportAsync(CancellationToken cancellationToken = default);
}
