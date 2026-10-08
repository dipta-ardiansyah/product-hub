using MediatR;
using Microsoft.Extensions.Logging;
using ProductHub.Application.Common.Abstractions;
using ProductHub.Application.Common.Exceptions;

namespace ProductHub.Application.Products.Commands
{
    public class DeleteProductCommandHandler : IRequestHandler<DeleteProductCommand, Unit>
    {
        private readonly IProductRepository _repository;
        private readonly ICacheVersionService _cacheVersionService;
        private readonly ILogger<DeleteProductCommandHandler> _logger;

        public DeleteProductCommandHandler(
            IProductRepository repository,
            ICacheVersionService cacheVersionService,
            ILogger<DeleteProductCommandHandler> logger)
        {
            _repository = repository;
            _cacheVersionService = cacheVersionService;
            _logger = logger;
        }

        public async Task<Unit> Handle(DeleteProductCommand request, CancellationToken cancellationToken)
        {
            var product = await _repository.GetByIdAsync(request.Id, cancellationToken);
            if (product == null)
            {
                throw new NotFoundException(nameof(product), request.Id);
            }

            await _repository.DeleteAsync(product, cancellationToken);
            await _cacheVersionService.IncrementVersionAsync(cancellationToken);

            _logger.LogInformation("Product deleted successfully");

            return Unit.Value;
        }
    }
}