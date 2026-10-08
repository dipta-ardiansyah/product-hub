namespace ProductHub.Application.Common.Abstractions
{
    public interface ICacheableQuery
    {
        string CacheKey { get; }
        TimeSpan? Expiration { get; }
    }
}