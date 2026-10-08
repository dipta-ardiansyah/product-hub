using MediatR;
using Microsoft.Extensions.Logging;
using ProductHub.Application.Common.Abstractions;
using ProductHub.Application.Common.Models;
using ProductHub.Application.Products.Dtos;

namespace ProductHub.Application.Products.Queries
{
    public class GetProductsQueryHandler : IRequestHandler<GetProductsQuery, PagedResult<ProductDto>>
    {
        private readonly IProductRepository _repository;
        private readonly ILogger<GetProductsQueryHandler> _logger;

        public GetProductsQueryHandler(
            IProductRepository repository,
            ILogger<GetProductsQueryHandler> logger)
        {
            _repository = repository;
            _logger = logger;
        }

        public async Task<PagedResult<ProductDto>> Handle(GetProductsQuery request, CancellationToken cancellationToken)
        {
            var (items, totalCount) = await _repository.SearchAsync(
                request.Name,
                request.MinPrice,
                request.MaxPrice,
                request.Page,
                request.PageSize,
                request.SortBy,
                request.SortDir.Equals("desc", StringComparison.OrdinalIgnoreCase),
                cancellationToken);

            _logger.LogInformation("Fetched {Count} products (Total: {TotalCount})", items.Count, totalCount);

            return new PagedResult<ProductDto>(items.Select(x => x.ToDto()).ToList(), totalCount, request.Page, request.PageSize);
        }
    }
}