using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Moq;
using ProductHub.Application.Auth.Commands;
using ProductHub.Application.Auth.Dtos;
using ProductHub.Application.Auth.Queries;
using ProductHub.Web.Controllers.API;

namespace ProductHub.UnitTests.Controllers;

public class AuthControllerTests
{
    private readonly Mock<IMediator> _mediatorMock;
    private readonly AuthController _controller;

    public AuthControllerTests()
    {
        _mediatorMock = new Mock<IMediator>();
        _controller = new AuthController(_mediatorMock.Object);
    }

    [Fact]
    public async Task Register_ValidCommand_ReturnsOkResultWithAuthResponse()
    {
        // Arrange
        var command = new RegisterUserCommand
        {
            Email = "test@example.com",
            Password = "Password123"
        };
        var expectedResponse = new AuthResponseDto
        {
            Token = "sample-jwt-token",
            UserId = "user-123",
            Email = "test@example.com"
        };

        _mediatorMock.Setup(m => m.Send(command, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResponse);

        // Act
        var result = await _controller.Register(command, CancellationToken.None);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.Value.Should().BeEquivalentTo(expectedResponse);
        _mediatorMock.Verify(m => m.Send(command, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Login_ValidQuery_ReturnsOkResultWithAuthResponse()
    {
        // Arrange
        var query = new LoginUserQuery
        {
            Email = "test@example.com",
            Password = "Password123"
        };
        var expectedResponse = new AuthResponseDto
        {
            Token = "sample-jwt-token",
            UserId = "user-123",
            Email = "test@example.com"
        };

        _mediatorMock.Setup(m => m.Send(query, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResponse);

        // Act
        var result = await _controller.Login(query, CancellationToken.None);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.Value.Should().BeEquivalentTo(expectedResponse);
        _mediatorMock.Verify(m => m.Send(query, It.IsAny<CancellationToken>()), Times.Once);
    }
}
