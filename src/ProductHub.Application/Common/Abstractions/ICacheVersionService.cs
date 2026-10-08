namespace ProductHub.Application.Common.Abstractions
{
    public interface ICacheVersionService
    {
        Task<int> GetAsync(CancellationToken cancellationToken);
        Task IncrementVersionAsync(CancellationToken cancellationToken);
    }
}