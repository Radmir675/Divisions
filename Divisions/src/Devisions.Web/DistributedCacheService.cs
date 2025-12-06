using System.Collections.Concurrent;
using System.Text.Json;
using Devisions.Application;
using Devisions.Application.Services;
using Microsoft.Extensions.Caching.Distributed;

namespace Devisions.Web;

public class DistributedCacheService : ICacheService
{
    private readonly IDistributedCache _cache;
    private readonly ConcurrentDictionary<string, bool> _cachedValues = new();

    public DistributedCacheService(IDistributedCache cache)
    {
        _cache = cache;
    }

    public async Task<T?> GetOrSetAsync<T>(string key, DistributedCacheEntryOptions options, Func<Task<T?>> factory,
        CancellationToken cancellationToken = default)
        where T : class
    {
        var cachedValue = await GetAsync<T>(key, cancellationToken);
        if (cachedValue is not null)
            return cachedValue;

        var newValue = await factory();

        if (newValue is not null)
        {
            await SetAsync(key, options, newValue, cancellationToken);
        }

        return newValue;
    }

    public async Task<T?> GetAsync<T>(
        string key,
        CancellationToken cancellationToken = default)
        where T : class
    {
        string? cachedValue = await _cache.GetStringAsync(key, cancellationToken);
        return cachedValue != null
            ? JsonSerializer.Deserialize<T>(cachedValue)
            : null;
    }

    public async Task SetAsync<T>(string key, DistributedCacheEntryOptions options, T value,
        CancellationToken cancellationToken = default)
        where T : class
    {
        string cachedValue = JsonSerializer.Serialize(value);
        await _cache.SetStringAsync(key, cachedValue, options, cancellationToken);
        _cachedValues.TryAdd(key, true);
    }

    public async Task RemoveAsync(string key, CancellationToken cancellationToken = default)
    {
        await _cache.RemoveAsync(key, cancellationToken);
        _cachedValues.TryRemove(key, out bool value);
    }

    public async Task RemoveByPrefixAsync(string prefixKey, CancellationToken cancellationToken = default)
    {
        var tasks = _cachedValues
            .Keys
            .Where(key => key.StartsWith(prefixKey))
            .Select(key => RemoveAsync(key, cancellationToken));

        await Task.WhenAll(tasks);
    }
}