using System.Text.Json;
using MediatR;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using ProductHub.Application.Common.Abstractions;

namespace ProductHub.Application.Common.Behaviors
{
    /// <summary>
    /// Cache-aside untuk query ICacheableQuery. Key memuat versi cache sehingga invalidasi
    /// cukup dengan mengganti versi. Kegagalan Redis TIDAK menggagalkan request.
    /// </summary>
    public class CachingBehavior<TRequest, TResponse>(
        IDistributedCache cache,
        ICacheVersionService versionService,
        ILogger<CachingBehavior<TRequest, TResponse>> logger)
        : IPipelineBehavior<TRequest, TResponse> where TRequest : notnull
    {
        public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
        {
            if (request is not ICacheableQuery query)
                return await next();

            string? key = null;
            try
            {
                var version = await versionService.GetAsync(cancellationToken);
                key = $"products:v{version}:{query.CacheKey}";

                var cached = await cache.GetStringAsync(key, cancellationToken);
                if (cached is not null)
                {
                    logger.LogDebug("Cache hit {CacheKey}", key);
                    return JsonSerializer.Deserialize<TResponse>(cached)!;
                }
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                logger.LogWarning(ex, "Cache read failed, falling back to database");
                key = null;
            }

            var response = await next();

            if (key is not null)
            {
                try
                {
                    await cache.SetStringAsync(key, JsonSerializer.Serialize(response),
                        new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = query.Expiration}, cancellationToken);
                }
                catch (Exception ex) when (ex is not OperationCanceledException)
                {
                    logger.LogWarning(ex, "Cache write failed");
                }
            }

            return response;
        }
    }
}