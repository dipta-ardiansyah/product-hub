namespace ProductHub.Application.Common.Abstractions
{
    public interface ICacheVersionService
    {
        Task<string> GetAsync(CancellationToken cancellationToken);
        Task IncrementVersionAsync(CancellationToken cancellationToken);
    }
}