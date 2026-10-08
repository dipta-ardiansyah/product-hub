using System.Text.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using ProductHub.Application.Common.Exceptions;
using ProductHub.Web.Middleware;
using Xunit;

namespace ProductHub.UnitTests.Middleware;

public class GlobalExceptionHandlerTests
{
    private readonly Mock<ILogger<GlobalExceptionHandler>> _loggerMock;
    private readonly GlobalExceptionHandler _handler;

    public GlobalExceptionHandlerTests()
    {
        _loggerMock = new Mock<ILogger<GlobalExceptionHandler>>();
        _handler = new GlobalExceptionHandler(_loggerMock.Object);
    }

    [Fact]
    public async Task TryHandleAsync_ValidationException_Returns400ProblemDetails()
    {
        // Arrange
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();

        var exception = new RequestValidationException("Name", "Name is required.");

        // Act
        var handled = await _handler.TryHandleAsync(context, exception, CancellationToken.None);

        // Assert
        handled.Should().BeTrue();
        context.Response.StatusCode.Should().Be(StatusCodes.Status400BadRequest);

        context.Response.Body.Seek(0, SeekOrigin.Begin);
        var responseText = await new StreamReader(context.Response.Body).ReadToEndAsync();
        var problem = JsonSerializer.Deserialize<ValidationProblemDetails>(responseText, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        problem.Should().NotBeNull();
        problem!.Errors.Should().ContainKey("Name");
        problem.Errors["Name"].Should().Contain("Name is required.");
    }

    [Fact]
    public async Task TryHandleAsync_NotFoundException_Returns404ProblemDetails()
    {
        // Arrange
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();

        var exception = new NotFoundException("Product", 10);

        // Act
        var handled = await _handler.TryHandleAsync(context, exception, CancellationToken.None);

        // Assert
        handled.Should().BeTrue();
        context.Response.StatusCode.Should().Be(StatusCodes.Status404NotFound);
    }

    [Fact]
    public async Task TryHandleAsync_UnauthorizedException_Returns401ProblemDetails()
    {
        // Arrange
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();

        var exception = new UnauthorizedException("Invalid credentials.");

        // Act
        var handled = await _handler.TryHandleAsync(context, exception, CancellationToken.None);

        // Assert
        handled.Should().BeTrue();
        context.Response.StatusCode.Should().Be(StatusCodes.Status401Unauthorized);
    }

    [Fact]
    public async Task TryHandleAsync_ConflictException_Returns409ProblemDetails()
    {
        // Arrange
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();

        var exception = new ConflictException("Email already taken.");

        // Act
        var handled = await _handler.TryHandleAsync(context, exception, CancellationToken.None);

        // Assert
        handled.Should().BeTrue();
        context.Response.StatusCode.Should().Be(StatusCodes.Status409Conflict);
    }

    [Fact]
    public async Task TryHandleAsync_UnknownException_Returns500ProblemDetails()
    {
        // Arrange
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();

        var exception = new InvalidOperationException("Unexpected failure.");

        // Act
        var handled = await _handler.TryHandleAsync(context, exception, CancellationToken.None);

        // Assert
        handled.Should().BeTrue();
        context.Response.StatusCode.Should().Be(StatusCodes.Status500InternalServerError);
    }
}
