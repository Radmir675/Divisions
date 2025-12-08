using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Distributed;

namespace Devisions.Application.Services;

public interface ICacheService
{
    Task<T?> GetOrSetAsync<T>(string key, DistributedCacheEntryOptions options, Func<Task<T?>> factory,
        CancellationToken cancellationToken = default)
        where T : class;

    Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default)
        where T : class;

    Task SetAsync<T>(string key, DistributedCacheEntryOptions options, T value,
        CancellationToken cancellationToken = default)
        where T : class;

    Task RemoveAsync(string key, CancellationToken cancellationToken = default);

    Task RemoveByPrefixAsync(string prefixKey, CancellationToken cancellationToken = default);
}