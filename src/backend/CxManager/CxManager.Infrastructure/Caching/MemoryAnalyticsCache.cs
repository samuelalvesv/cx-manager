using System.Collections.Concurrent;
using CxManager.Application.Interfaces;

namespace CxManager.Infrastructure.Caching;

public sealed class MemoryAnalyticsCache : IAnalyticsCache
{
    private readonly ConcurrentDictionary<long, ConcurrentDictionary<string, object>> _companyCaches = new();

    public T? Get<T>(long companyId, string key)
    {
        if (_companyCaches.TryGetValue(key: companyId, value: out ConcurrentDictionary<string, object>? cache) &&
            cache.TryGetValue(key: key, value: out object? value) &&
            value is T typedValue)
        {
            return typedValue;
        }

        return default;
    }

    public void Set<T>(long companyId, string key, T value)
    {
        if (value is null)
        {
            return;
        }

        ConcurrentDictionary<string, object> cache = _companyCaches.GetOrAdd(
            key: companyId,
            valueFactory: static _ => new ConcurrentDictionary<string, object>());

        cache[key] = value;
    }

    public void InvalidateCompany(long companyId) => _companyCaches.TryRemove(key: companyId, value: out _);
}
