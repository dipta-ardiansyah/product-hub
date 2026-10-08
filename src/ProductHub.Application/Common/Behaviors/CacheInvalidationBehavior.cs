using MediatR;
using Microsoft.Extensions.Logging;
using ProductHub.Application.Common.Abstractions;

namespace ProductHub.Application.Common.Behaviors
{
    public class CacheInvalidationBehavior<TRequest, TResponse>(
        ICacheVersionService versionService,
        ILogger<CacheInvalidationBehavior<TRequest, TResponse>> logger)
        : IPipelineBehavior<TRequest, TResponse> where TRequest : notnull
    {
        public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
        {
            var response = await next();

            if (request is IInvalidatesProductCache)
            {
                try { await versionService.IncrementVersionAsync(cancellationToken); }
                catch (Exception ex) when (ex is not OperationCanceledException)
                {
                    logger.LogWarning(ex, "Failed to invalidate product cache; entries expire by Expiration");
                }
            }

            return response;
        }
    }
}