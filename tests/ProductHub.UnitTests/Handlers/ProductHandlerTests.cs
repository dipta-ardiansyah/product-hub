using FluentAssertions;
using MediatR;
using Microsoft.Extensions.Logging;
using Moq;
using ProductHub.Application.Common.Abstractions;
using ProductHub.Application.Common.Exceptions;
using ProductHub.Application.Products.Commands;
using ProductHub.Application.Products.Queries;
using ProductHub.Domain.Entities;

namespace ProductHub.UnitTests.Handlers;

public class ProductHandlerTests
{
    private readonly Mock<IProductRepository> _repoMock;
    private readonly Mock<ICacheVersionService> _cacheVersionMock;

    public ProductHandlerTests()
    {
        _repoMock = new Mock<IProductRepository>();
        _cacheVersionMock = new Mock<ICacheVersionService>();
    }

    [Fact]
    public async Task CreateProductHandler_ValidRequest_CreatesProductAndIncrementsCache()
    {
        // Arrange
        var command = new CreateProductCommand { Name = "Desk", Description = "Wooden Desk", Price = 150 };
        _repoMock.Setup(r => r.AddAsync(It.IsAny<Product>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Product p, CancellationToken ct) =>
            {
                p.Id = 10;
                return p;
            });

        var loggerMock = new Mock<ILogger<CreateProductCommandHandler>>();
        var handler = new CreateProductCommandHandler(_repoMock.Object, _cacheVersionMock.Object, loggerMock.Object);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Id.Should().Be(10);
        result.Name.Should().Be("Desk");
        result.Price.Should().Be(150);
        _cacheVersionMock.Verify(c => c.IncrementVersionAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpdateProductHandler_ExistingProduct_UpdatesAndIncrementsCache()
    {
        // Arrange
        var existing = new Product { Id = 1, Name = "Old Name", Price = 50, CreatedAt = DateTime.UtcNow };
        _repoMock.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(existing);

        var command = new UpdateProductCommand { Id = 1, Name = "New Name", Description = "Desc", Price = 75 };
        var loggerMock = new Mock<ILogger<UpdateProductCommandHandler>>();
        var handler = new UpdateProductCommandHandler(_repoMock.Object, _cacheVersionMock.Object, loggerMock.Object);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Name.Should().Be("New Name");
        result.Price.Should().Be(75);
        _repoMock.Verify(r => r.UpdateAsync(existing, It.IsAny<CancellationToken>()), Times.Once);
        _cacheVersionMock.Verify(c => c.IncrementVersionAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpdateProductHandler_NotFound_ThrowsNotFoundException()
    {
        // Arrange
        _repoMock.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync((Product?)null);

        var command = new UpdateProductCommand { Id = 1, Name = "New Name", Price = 75 };
        var loggerMock = new Mock<ILogger<UpdateProductCommandHandler>>();
        var handler = new UpdateProductCommandHandler(_repoMock.Object, _cacheVersionMock.Object, loggerMock.Object);

        // Act & Assert
        await handler.Invoking(h => h.Handle(command, CancellationToken.None))
            .Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task DeleteProductHandler_ExistingProduct_DeletesAndIncrementsCache()
    {
        // Arrange
        var existing = new Product { Id = 1, Name = "Old Name", Price = 50, CreatedAt = DateTime.UtcNow };
        _repoMock.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(existing);

        var command = new DeleteProductCommand(1);
        var loggerMock = new Mock<ILogger<DeleteProductCommandHandler>>();
        var handler = new DeleteProductCommandHandler(_repoMock.Object, _cacheVersionMock.Object, loggerMock.Object);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().Be(Unit.Value);
        _repoMock.Verify(r => r.DeleteAsync(existing, It.IsAny<CancellationToken>()), Times.Once);
        _cacheVersionMock.Verify(c => c.IncrementVersionAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DeleteProductHandler_NotFound_ThrowsNotFoundException()
    {
        // Arrange
        _repoMock.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync((Product?)null);

        var command = new DeleteProductCommand(1);
        var loggerMock = new Mock<ILogger<DeleteProductCommandHandler>>();
        var handler = new DeleteProductCommandHandler(_repoMock.Object, _cacheVersionMock.Object, loggerMock.Object);

        // Act & Assert
        await handler.Invoking(h => h.Handle(command, CancellationToken.None))
            .Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task GetProductByIdHandler_ExistingId_ReturnsProduct()
    {
        // Arrange
        var product = new Product { Id = 2, Name = "Chair", Description = "Ergonomic", Price = 250, CreatedAt = DateTime.UtcNow };
        _repoMock.Setup(r => r.GetByIdAsync(2, It.IsAny<CancellationToken>())).ReturnsAsync(product);

        var loggerMock = new Mock<ILogger<GetProductByIdQueryHandler>>();
        var handler = new GetProductByIdQueryHandler(_repoMock.Object, loggerMock.Object);

        // Act
        var result = await handler.Handle(new GetProductByIdQuery(2), CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(2);
        result.Name.Should().Be("Chair");
    }

    [Fact]
    public async Task GetProductsHandler_ValidQuery_ReturnsPagedResult()
    {
        // Arrange
        var items = new List<Product>
        {
            new() { Id = 1, Name = "Item 1", Price = 10, CreatedAt = DateTime.UtcNow },
            new() { Id = 2, Name = "Item 2", Price = 20, CreatedAt = DateTime.UtcNow }
        };
        _repoMock.Setup(r => r.SearchAsync(null, null, null, 1, 10, null, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync((items, 2));

        var loggerMock = new Mock<ILogger<GetProductsQueryHandler>>();
        var handler = new GetProductsQueryHandler(_repoMock.Object, loggerMock.Object);

        // Act
        var result = await handler.Handle(new GetProductsQuery { Page = 1, PageSize = 10 }, CancellationToken.None);

        // Assert
        result.Items.Should().HaveCount(2);
        result.TotalCount.Should().Be(2);
        result.Page.Should().Be(1);
    }
}
