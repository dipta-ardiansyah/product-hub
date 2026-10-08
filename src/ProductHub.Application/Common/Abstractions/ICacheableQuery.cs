using MediatR;

namespace ProductHub.Application.Common.Abstractions
{
    public interface ICacheableQuery<out TResponse> : IRequest<TResponse>
    {
        string CacheKey { get; }
        TimeSpan? Expiration { get; }
    }
}