using MediatR;
using Microsoft.Extensions.Logging;
using ProductHub.Application.Common.Abstractions;
using ProductHub.Application.Common.Exceptions;
using ProductHub.Application.Products.Dtos;

namespace ProductHub.Application.Products.Commands
{
    public class UpdateProductCommandHandler : IRequestHandler<UpdateProductCommand, ProductDto>
    {
        private readonly IProductRepository _repository;
        private readonly ICacheVersionService _cacheVersionService;
        private readonly ILogger<UpdateProductCommandHandler> _logger;

        public UpdateProductCommandHandler(
            IProductRepository repository,
            ICacheVersionService cacheVersionService,
            ILogger<UpdateProductCommandHandler> logger)
        {
            _repository = repository;
            _cacheVersionService = cacheVersionService;
            _logger = logger;
        }

        public async Task<ProductDto> Handle(UpdateProductCommand request, CancellationToken cancellationToken)
        {
            var product = await _repository.GetByIdAsync(request.Id, cancellationToken);
            if (product == null)
            {
                throw new NotFoundException(nameof(product), request.Id);
            }

            product.Name = request.Name.Trim();
            product.Description = request.Description.Trim();
            product.Price = request.Price;

            await _repository.UpdateAsync(product, cancellationToken);
            await _cacheVersionService.IncrementVersionAsync(cancellationToken);

            _logger.LogInformation("Product updated successfully");

            return product.ToDto();
        }
    }
}