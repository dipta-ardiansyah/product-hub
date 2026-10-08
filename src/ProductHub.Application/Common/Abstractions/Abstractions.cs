using ProductHub.Application.Auth;
using ProductHub.Domain.Entities;

namespace ProductHub.Application.Common.Abstractions
{
    public record ProductSearchCriteria(
        string? Name, decimal? MinPrice, decimal? MaxPrice,
        int Page, int PageSize, string SortBy, bool Descending);

    public interface IProductRepository
    {
        Task<Product?> GetByIdAsync(int id, CancellationToken cancellationToken);
        Task<(IReadOnlyList<Product> Items, int TotalCount)> SearchAsync(ProductSearchCriteria criteria, CancellationToken ct);
        Task AddAsync(Product product, CancellationToken cancellationToken);
        Task UpdateAsync(Product product, CancellationToken cancellationToken);
        Task DeleteAsync(Product product, CancellationToken cancellationToken);
    }

    public interface IIdentityService
    {
        Task<AuthResponse> RegisterAsync(string email, string password, CancellationToken cancellationToken);
        Task<AuthResponse> LoginAsync(string email, string password, CancellationToken cancellationToken);
    }

    public interface IJwtTokenService
    {
        AuthResponse CreateToken(string userId, string email);
    }

    public interface ICacheVersionService
    {
        Task<string> GetAsync(CancellationToken cancellationToken);
        Task BumpAsync(CancellationToken cancellationToken);
    }

    /// <summary>Query yang hasilnya boleh di-cache.</summary>
    public interface ICacheableQuery
    {
        string CacheKey { get; }
        TimeSpan Ttl { get; }
    }

    /// <summary>Command yang membuat cache produk menjadi tidak terpakai (versi key dinaikkan setelah sukses).</summary>
    public interface IInvalidatesProductCache { }
}