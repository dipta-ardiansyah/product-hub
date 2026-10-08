using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Moq;
using ProductHub.Application.Common.Exceptions;
using ProductHub.Application.Common.Models;
using ProductHub.Application.Products.Commands;
using ProductHub.Application.Products.Dtos;
using ProductHub.Application.Products.Queries;
using ProductHub.Web.Controllers.API;

namespace ProductHub.UnitTests.Controllers;

public class ProductsControllerTests
{
    private readonly Mock<IMediator> _mediatorMock;
    private readonly ProductsController _controller;

    public ProductsControllerTests()
    {
        _mediatorMock = new Mock<IMediator>();
        _controller = new ProductsController(_mediatorMock.Object);
    }

    [Fact]
    public async Task GetAll_ValidQuery_ReturnsOkWithPagedResult()
    {
        // Arrange
        var query = new GetProductsQuery { Page = 1, PageSize = 10, Name = "Laptop" };
        var pagedResult = new PagedResult<ProductDto>(
            new List<ProductDto>
            {
                new() { Id = 1, Name = "Laptop Pro", Price = 1200, CreatedAt = DateTime.UtcNow }
            },
            1, 1, 10);

        _mediatorMock.Setup(m => m.Send(query, It.IsAny<CancellationToken>()))
            .ReturnsAsync(pagedResult);

        // Act
        var result = await _controller.GetAll(query, CancellationToken.None);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.Value.Should().BeEquivalentTo(pagedResult);
    }

    [Fact]
    public async Task GetById_ExistingId_ReturnsOkWithProduct()
    {
        // Arrange
        var id = 1;
        var expectedProduct = new ProductDto
        {
            Id = id,
            Name = "Smartphone",
            Description = "Latest model",
            Price = 999.99m,
            CreatedAt = DateTime.UtcNow
        };

        _mediatorMock.Setup(m => m.Send(It.Is<GetProductByIdQuery>(q => q.Id == id), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedProduct);

        // Act
        var result = await _controller.GetById(id, CancellationToken.None);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.Value.Should().BeEquivalentTo(expectedProduct);
    }

    [Fact]
    public async Task GetById_NotFound_ThrowsNotFoundException()
    {
        // Arrange
        var id = 99;
        _mediatorMock.Setup(m => m.Send(It.Is<GetProductByIdQuery>(q => q.Id == id), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new NotFoundException("Product", id));

        // Act & Assert
        await _controller.Invoking(c => c.GetById(id, CancellationToken.None))
            .Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Create_ValidCommand_ReturnsCreatedAtActionResult()
    {
        // Arrange
        var command = new CreateProductCommand
        {
            Name = "Monitor 4K",
            Description = "32 inch 144Hz",
            Price = 450.00m
        };

        var createdProduct = new ProductDto
        {
            Id = 5,
            Name = command.Name,
            Description = command.Description,
            Price = command.Price,
            CreatedAt = DateTime.UtcNow
        };

        _mediatorMock.Setup(m => m.Send(command, It.IsAny<CancellationToken>()))
            .ReturnsAsync(createdProduct);

        // Act
        var result = await _controller.Create(command, CancellationToken.None);

        // Assert
        var createdResult = result.Should().BeOfType<CreatedAtActionResult>().Subject;
        createdResult.ActionName.Should().Be(nameof(ProductsController.GetById));
        createdResult.RouteValues!["id"].Should().Be(5);
        createdResult.Value.Should().BeEquivalentTo(createdProduct);
    }

    [Fact]
    public async Task Update_ValidCommand_ReturnsOkResult()
    {
        // Arrange
        var id = 5;
        var command = new UpdateProductCommand
        {
            Id = id,
            Name = "Monitor 4K OLED",
            Description = "Updated description",
            Price = 500.00m
        };

        var updatedProduct = new ProductDto
        {
            Id = id,
            Name = command.Name,
            Description = command.Description,
            Price = command.Price,
            CreatedAt = DateTime.UtcNow
        };

        _mediatorMock.Setup(m => m.Send(command, It.IsAny<CancellationToken>()))
            .ReturnsAsync(updatedProduct);

        // Act
        var result = await _controller.Update(id, command, CancellationToken.None);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.Value.Should().BeEquivalentTo(updatedProduct);
        command.Id.Should().Be(id);
    }

    [Fact]
    public async Task Delete_ExistingId_ReturnsNoContentResult()
    {
        // Arrange
        var id = 5;
        _mediatorMock.Setup(m => m.Send(It.Is<DeleteProductCommand>(c => c.Id == id), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Unit.Value);

        // Act
        var result = await _controller.Delete(id, CancellationToken.None);

        // Assert
        result.Should().BeOfType<NoContentResult>();
        _mediatorMock.Verify(m => m.Send(It.Is<DeleteProductCommand>(c => c.Id == id), It.IsAny<CancellationToken>()), Times.Once);
    }
}
