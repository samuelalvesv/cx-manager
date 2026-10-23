using System.Collections.Concurrent;
using CxManager.Application.Interfaces;

namespace CxManager.Infrastructure.Caching;

public sealed class MemoryAnalyticsCache : IAnalyticsCache
{
    private static readonly TimeSpan DefaultTtl = TimeSpan.FromMinutes(15);
    private const int MaxEntriesPerCompany = 500;

    private readonly record struct CacheEntry(object Value, DateTimeOffset ExpiresAt);

    private readonly ConcurrentDictionary<long, ConcurrentDictionary<string, CacheEntry>> _companyCaches = new();

    public T? Get<T>(long companyId, string key)
    {
        if (!_companyCaches.TryGetValue(key: companyId, value: out ConcurrentDictionary<string, CacheEntry>? cache) ||
            !cache.TryGetValue(key: key, value: out CacheEntry entry))
            return default;

        if (entry.ExpiresAt > DateTimeOffset.UtcNow && entry.Value is T typedValue)
            return typedValue;

        cache.TryRemove(key: key, value: out _);

        return default;
    }

    public void Set<T>(long companyId, string key, T value)
    {
        if (value is null)
            return;

        ConcurrentDictionary<string, CacheEntry> cache = _companyCaches.GetOrAdd(
            key: companyId,
            valueFactory: static _ => new ConcurrentDictionary<string, CacheEntry>());

        if (cache.Count >= MaxEntriesPerCompany)
            cache.Clear();

        cache[key] = new CacheEntry(Value: value, ExpiresAt: DateTimeOffset.UtcNow.Add(DefaultTtl));
    }

    public void InvalidateCompany(long companyId) => _companyCaches.TryRemove(key: companyId, value: out _);
}
