using System.Text.Json;
using MediatR;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using ProductHub.Application.Common.Abstractions;

namespace ProductHub.Application.Common.Behaviors
{
    public class CachingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : ICacheableQuery<TResponse>
    {
        private readonly IDistributedCache _cache;
        private readonly ICacheVersionService _cacheVersionService;
        private readonly ILogger<CachingBehavior<TRequest, TResponse>> _logger;

        public CachingBehavior(
            IDistributedCache cache,
            ICacheVersionService cacheVersionService,
            ILogger<CachingBehavior<TRequest, TResponse>> logger)
        {
            _cache = cache;
            _cacheVersionService = cacheVersionService;
            _logger = logger;
        }

        public async Task<TResponse> Handle(
            TRequest request,
            RequestHandlerDelegate<TResponse> next,
            CancellationToken cancellationToken)
        {
            string versionedKey;
            try
            {
                var version = await _cacheVersionService.GetAsync(cancellationToken);
                versionedKey = $"products:v{version}:{request.CacheKey}";

                var cachedData = await _cache.GetStringAsync(versionedKey, cancellationToken);
                if (!string.IsNullOrEmpty(cachedData))
                {
                    var deserialized = JsonSerializer.Deserialize<TResponse>(cachedData);
                    if (deserialized != null)
                    {
                        _logger.LogInformation("Cache hit for key {CacheKey}", versionedKey);
                        return deserialized;
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Cache retrieval failed for request {RequestType}. Falling back to source.", typeof(TRequest).Name);
                return await next();
            }

            var response = await next();

            try
            {
                var options = new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = request.Expiration ?? TimeSpan.FromMinutes(2)
                };

                var serialized = JsonSerializer.Serialize(response);
                await _cache.SetStringAsync(versionedKey, serialized, options, cancellationToken);
                _logger.LogInformation("Cache set for key {CacheKey}", versionedKey);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Cache write failed for key {CacheKey}", versionedKey);
            }

            return response;
        }
    }
}