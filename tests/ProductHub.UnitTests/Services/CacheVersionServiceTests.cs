using FluentAssertions;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Moq;
using ProductHub.Infrastructure.Caching;
using Xunit;

namespace ProductHub.UnitTests.Services;

public class CacheVersionServiceTests
{
    private readonly Mock<IDistributedCache> _cacheMock;
    private readonly Mock<ILogger<CacheVersionService>> _loggerMock;
    private readonly CacheVersionService _service;

    public CacheVersionServiceTests()
    {
        _cacheMock = new Mock<IDistributedCache>();
        _loggerMock = new Mock<ILogger<CacheVersionService>>();
        _service = new CacheVersionService(_cacheMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task GetVersionAsync_NoKeyInCache_ReturnsDefaultOne()
    {
        // Arrange
        _cacheMock.Setup(c => c.GetAsync("products:version", It.IsAny<CancellationToken>()))
            .ReturnsAsync((byte[]?)null);

        // Act
        var version = await _service.GetAsync();

        // Assert
        version.Should().Be(1);
    }

    [Fact]
    public async Task GetVersionAsync_ValidKeyInCache_ReturnsParsedVersion()
    {
        // Arrange
        var bytes = System.Text.Encoding.UTF8.GetBytes("5");
        _cacheMock.Setup(c => c.GetAsync("products:version", It.IsAny<CancellationToken>()))
            .ReturnsAsync(bytes);

        // Act
        var version = await _service.GetAsync();

        // Assert
        version.Should().Be(5);
    }

    [Fact]
    public async Task IncrementVersionAsync_IncrementsVersionInCache()
    {
        // Arrange
        var bytes = System.Text.Encoding.UTF8.GetBytes("3");
        _cacheMock.Setup(c => c.GetAsync("products:version", It.IsAny<CancellationToken>()))
            .ReturnsAsync(bytes);

        // Act
        await _service.IncrementVersionAsync();

        // Assert
        _cacheMock.Verify(c => c.SetAsync(
            "products:version",
            System.Text.Encoding.UTF8.GetBytes("4"),
            It.IsAny<DistributedCacheEntryOptions>(),
            It.IsAny<CancellationToken>()), Times.Once);
    }
}
