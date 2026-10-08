using System.Text;
using System.Text.Json;
using FluentAssertions;
using MediatR;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Moq;
using ProductHub.Application.Common.Abstractions;
using ProductHub.Application.Common.Behaviors;
using ProductHub.Application.Common.Exceptions;
using ProductHub.Application.Products.Commands;
using ProductHub.Application.Products.Dtos;
using ProductHub.Application.Products.Queries;
using Xunit;

namespace ProductHub.UnitTests.Behaviors;

public class BehaviorTests
{
    [Fact]
    public async Task ValidationBehavior_ValidObject_CallsNext()
    {
        // Arrange
        var behavior = new ValidationBehavior<CreateProductCommand, ProductDto>();
        var command = new CreateProductCommand { Name = "Valid Name", Price = 100 };
        var expectedResponse = new ProductDto { Id = 1, Name = "Valid Name", Price = 100 };

        RequestHandlerDelegate<ProductDto> next = new RequestHandlerDelegate<ProductDto>(() => Task.FromResult(expectedResponse));

        // Act
        var result = await behavior.Handle(command, next, CancellationToken.None);

        // Assert
        result.Should().BeEquivalentTo(expectedResponse);
    }

    [Fact]
    public async Task ValidationBehavior_InvalidObject_ThrowsValidationException()
    {
        // Arrange
        var behavior = new ValidationBehavior<CreateProductCommand, ProductDto>();
        var invalidCommand = new CreateProductCommand { Name = "", Price = -5 }; // Required name, price < 0.01

        RequestHandlerDelegate<ProductDto> next = new RequestHandlerDelegate<ProductDto>(() => Task.FromResult(new ProductDto()));

        // Act & Assert
        await behavior.Invoking(b => b.Handle(invalidCommand, next, CancellationToken.None))
            .Should().ThrowAsync<RequestValidationException>();
    }

    [Fact]
    public async Task CachingBehavior_CacheHit_ReturnsCachedResponseWithoutCallingNext()
    {
        // Arrange
        var cacheMock = new Mock<IDistributedCache>();
        var versionServiceMock = new Mock<ICacheVersionService>();
        var loggerMock = new Mock<ILogger<CachingBehavior<GetProductByIdQuery, ProductDto>>>();

        versionServiceMock.Setup(v => v.GetAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        var cachedProduct = new ProductDto { Id = 1, Name = "Cached Desk", Price = 200 };
        var serialized = JsonSerializer.Serialize(cachedProduct);

        cacheMock.Setup(c => c.GetAsync("products:v1:item:1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(Encoding.UTF8.GetBytes(serialized));

        var behavior = new CachingBehavior<GetProductByIdQuery, ProductDto>(
            cacheMock.Object, versionServiceMock.Object, loggerMock.Object);

        bool nextCalled = false;
        RequestHandlerDelegate<ProductDto> next = new RequestHandlerDelegate<ProductDto>(() =>
        {
            nextCalled = true;
            return Task.FromResult(new ProductDto { Id = 1, Name = "From DB", Price = 200 });
        });

        // Act
        var result = await behavior.Handle(new GetProductByIdQuery(1), next, CancellationToken.None);

        // Assert
        nextCalled.Should().BeFalse();
        result.Should().NotBeNull();
        result.Name.Should().Be("Cached Desk");
    }

    [Fact]
    public async Task CachingBehavior_CacheMiss_CallsNextAndSetsCache()
    {
        // Arrange
        var cacheMock = new Mock<IDistributedCache>();
        var versionServiceMock = new Mock<ICacheVersionService>();
        var loggerMock = new Mock<ILogger<CachingBehavior<GetProductByIdQuery, ProductDto>>>();

        versionServiceMock.Setup(v => v.GetAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        cacheMock.Setup(c => c.GetAsync("products:v1:item:1", It.IsAny<CancellationToken>()))
            .ReturnsAsync((byte[]?)null);

        var behavior = new CachingBehavior<GetProductByIdQuery, ProductDto>(
            cacheMock.Object, versionServiceMock.Object, loggerMock.Object);

        var dbProduct = new ProductDto { Id = 1, Name = "From DB", Price = 200 };
        RequestHandlerDelegate<ProductDto> next = new RequestHandlerDelegate<ProductDto>(() => Task.FromResult(dbProduct));

        // Act
        var result = await behavior.Handle(new GetProductByIdQuery(1), next, CancellationToken.None);

        // Assert
        result.Should().BeEquivalentTo(dbProduct);
        cacheMock.Verify(c => c.SetAsync(
            "products:v1:item:1",
            It.IsAny<byte[]>(),
            It.IsAny<DistributedCacheEntryOptions>(),
            It.IsAny<CancellationToken>()), Times.Once);
    }
}
