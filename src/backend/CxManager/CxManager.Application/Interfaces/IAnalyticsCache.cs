namespace CxManager.Application.Interfaces;

public interface IAnalyticsCache
{
    T? Get<T>(long companyId, string key);
    void Set<T>(long companyId, string key, T value);
    void InvalidateCompany(long companyId);
}
