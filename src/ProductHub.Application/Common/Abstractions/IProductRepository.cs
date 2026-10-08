using ProductHub.Domain.Entities;

namespace ProductHub.Application.Common.Abstractions
{
    public interface IProductRepository
    {
        Task<Product?> GetByIdAsync(int id, CancellationToken cancellationToken);
        Task<(IReadOnlyList<Product> Items, int TotalCount)> SearchAsync(
            string? Name,
            decimal? MinPrice,
            decimal? MaxPrice,
            int Page,
            int PageSize,
            string SortBy,
            bool Descending,
            CancellationToken cancellationToken);
        Task<Product> AddAsync(Product product, CancellationToken cancellationToken);
        Task UpdateAsync(Product product, CancellationToken cancellationToken);
        Task DeleteAsync(Product product, CancellationToken cancellationToken);
    }
}