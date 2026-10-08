using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using ProductHub.Application.Common.Abstractions;

namespace ProductHub.Infrastructure.Caching
{
    public class CacheVersionService : ICacheVersionService
    {
        private const string VersionKey = "products:version";
        private readonly IDistributedCache _cache;
        private readonly ILogger<CacheVersionService> _logger;

        public CacheVersionService(IDistributedCache cache, ILogger<CacheVersionService> logger)
        {
            _cache = cache;
            _logger = logger;
        }

        public async Task<int> GetAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                var value = await _cache.GetStringAsync(VersionKey, cancellationToken);
                if (!string.IsNullOrEmpty(value) && int.TryParse(value, out var version))
                {
                    return version;
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to read cache version from Redis. Defaulting to 1.");
            }

            return 1;
        }

        public async Task IncrementVersionAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                var currentVersion = await GetAsync(cancellationToken);
                var nextVersion = currentVersion + 1;
                await _cache.SetStringAsync(VersionKey, nextVersion.ToString(), cancellationToken);
                _logger.LogInformation("Cache version incremented to {Version}", nextVersion);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to increment cache version in Redis.");
            }
        }
    }
}