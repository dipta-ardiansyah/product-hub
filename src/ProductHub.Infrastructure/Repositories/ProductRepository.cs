using Microsoft.EntityFrameworkCore;
using ProductHub.Application.Common.Abstractions;
using ProductHub.Domain.Entities;
using ProductHub.Infrastructure.Persistence;

namespace ProductHub.Infrastructure.Repositories
{
    public class ProductRepository : IProductRepository
    {
        private readonly AppDbContext _context;

        public ProductRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<Product?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            return await _context.Products
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
        }

        public async Task<(IReadOnlyList<Product> Items, int TotalCount)> SearchAsync(
            string? name,
            decimal? minPrice,
            decimal? maxPrice,
            int page,
            int pageSize,
            string? sortBy,
            string? sortDir,
            CancellationToken cancellationToken = default)
        {
            var query = _context.Products.AsNoTracking().AsQueryable();

            if (!string.IsNullOrWhiteSpace(name))
            {
                var searchTerm = name.Trim().ToLower();
                query = query.Where(p => p.Name.ToLower().Contains(searchTerm));
            }

            if (minPrice.HasValue)
            {
                query = query.Where(p => p.Price >= minPrice.Value);
            }

            if (maxPrice.HasValue)
            {
                query = query.Where(p => p.Price <= maxPrice.Value);
            }

            var isDescending = string.Equals(sortDir, "desc", StringComparison.OrdinalIgnoreCase);

            query = sortBy?.ToLower() switch
            {
                "name" => isDescending ? query.OrderByDescending(p => p.Name) : query.OrderBy(p => p.Name),
                "price" => isDescending ? query.OrderByDescending(p => p.Price) : query.OrderBy(p => p.Price),
                _ => isDescending ? query.OrderByDescending(p => p.CreatedAt) : query.OrderBy(p => p.CreatedAt)
            };

            var totalCount = await query.CountAsync(cancellationToken);

            var pageNumber = page < 1 ? 1 : page;
            var validPageSize = pageSize < 1 ? 10 : (pageSize > 100 ? 100 : pageSize);

            var items = await query
                .Skip((pageNumber - 1) * validPageSize)
                .Take(validPageSize)
                .ToListAsync(cancellationToken);

            return (items, totalCount);
        }

        public async Task<Product> AddAsync(Product product, CancellationToken cancellationToken = default)
        {
            _context.Products.Add(product);
            await _context.SaveChangesAsync(cancellationToken);
            return product;
        }

        public async Task UpdateAsync(Product product, CancellationToken cancellationToken = default)
        {
            _context.Entry(product).State = EntityState.Modified;
            await _context.SaveChangesAsync(cancellationToken);
        }

        public async Task DeleteAsync(Product product, CancellationToken cancellationToken = default)
        {
            _context.Products.Remove(product);
            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}