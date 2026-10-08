using MediatR;
using Microsoft.Extensions.Logging;
using ProductHub.Application.Common.Abstractions;
using ProductHub.Application.Common.Exceptions;
using ProductHub.Application.Products.Dtos;

namespace ProductHub.Application.Products.Queries
{
    public class GetProductByIdQueryHandler : IRequestHandler<GetProductByIdQuery, ProductDto>
    {
        private readonly IProductRepository _repository;
        private readonly ILogger<GetProductByIdQueryHandler> _logger;

        public GetProductByIdQueryHandler(
            IProductRepository repository,
            ILogger<GetProductByIdQueryHandler> logger)
        {
            _repository = repository;
            _logger = logger;
        }

        public async Task<ProductDto> Handle(GetProductByIdQuery request, CancellationToken cancellationToken)
        {
            var product = await _repository.GetByIdAsync(request.Id, cancellationToken);
            if (product == null)
            {
                _logger.LogWarning("Product {ProductId} not found", request.Id);
                throw new NotFoundException(nameof(product), request.Id);
            }

            return product.ToDto();
        }
    }
}