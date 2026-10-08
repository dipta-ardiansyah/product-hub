using MediatR;
using Microsoft.Extensions.Logging;
using ProductHub.Application.Common.Abstractions;
using ProductHub.Application.Products.Dtos;
using ProductHub.Domain.Entities;

namespace ProductHub.Application.Products.Commands
{
    public class CreateProductCommandHandler : IRequestHandler<CreateProductCommand, ProductDto>
    {
        private readonly IProductRepository _repository;
        private readonly ICacheVersionService _cacheVersionService;
        private readonly ILogger<CreateProductCommandHandler> _logger;

        public CreateProductCommandHandler(
            IProductRepository repository,
            ICacheVersionService cacheVersionService,
            ILogger<CreateProductCommandHandler> logger)
        {
            _repository = repository;
            _cacheVersionService = cacheVersionService;
            _logger = logger;
        }

        public async Task<ProductDto> Handle(CreateProductCommand request, CancellationToken cancellationToken)
        {
            var product = new Product
            {
                Name = request.Name.Trim(),
                Description = request.Description.Trim(),
                Price = request.Price,
                CreatedAt = DateTime.UtcNow
            };

            var created = await _repository.AddAsync(product, cancellationToken);
            await _cacheVersionService.IncrementVersionAsync(cancellationToken);

            _logger.LogInformation("Product created successfully");

            return created.ToDto();
        }
    }
}