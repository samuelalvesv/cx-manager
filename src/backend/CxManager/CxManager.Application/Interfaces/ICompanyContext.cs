namespace CxManager.Application.Interfaces;

public interface ICompanyContext
{
    long CompanyId { get; }
    bool HasCompanyId { get; }
    long? TryGetCompanyId();
    void EnsureCompanyId();
}
