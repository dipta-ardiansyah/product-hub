using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using ProductHub.Application.Auth.Commands;
using ProductHub.Application.Auth.Queries;
using ProductHub.Application.Common.Abstractions;
using ProductHub.Application.Common.Exceptions;

namespace ProductHub.UnitTests.Handlers;

public class AuthHandlerTests
{
    private readonly Mock<IIdentityService> _identityServiceMock;
    private readonly Mock<IJwtTokenService> _jwtTokenServiceMock;
    private readonly Mock<ILogger<RegisterUserCommandHandler>> _registerLoggerMock;
    private readonly Mock<ILogger<LoginUserQueryHandler>> _loginLoggerMock;

    public AuthHandlerTests()
    {
        _identityServiceMock = new Mock<IIdentityService>();
        _jwtTokenServiceMock = new Mock<IJwtTokenService>();
        _registerLoggerMock = new Mock<ILogger<RegisterUserCommandHandler>>();
        _loginLoggerMock = new Mock<ILogger<LoginUserQueryHandler>>();
    }

    [Fact]
    public async Task RegisterUserHandler_SuccessfulRegistration_ReturnsTokenAndUserInfo()
    {
        // Arrange
        var command = new RegisterUserCommand { Email = "user@example.com", Password = "Password123" };
        _identityServiceMock.Setup(s => s.RegisterAsync(command.Email, command.Password, It.IsAny<CancellationToken>()))
            .ReturnsAsync((true, "user-id-1", Array.Empty<string>()));

        _jwtTokenServiceMock.Setup(j => j.CreateToken("user-id-1", command.Email, It.IsAny<IEnumerable<string>>()))
            .Returns("generated-jwt");

        var handler = new RegisterUserCommandHandler(_identityServiceMock.Object, _jwtTokenServiceMock.Object, _registerLoggerMock.Object);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Token.Should().Be("generated-jwt");
        result.UserId.Should().Be("user-id-1");
        result.Email.Should().Be("user@example.com");
    }

    [Fact]
    public async Task RegisterUserHandler_DuplicateEmail_ThrowsConflictException()
    {
        // Arrange
        var command = new RegisterUserCommand { Email = "user@example.com", Password = "Password123" };
        _identityServiceMock.Setup(s => s.RegisterAsync(command.Email, command.Password, It.IsAny<CancellationToken>()))
            .ReturnsAsync((false, string.Empty, new[] { "An account with this email already taken." }));

        var handler = new RegisterUserCommandHandler(_identityServiceMock.Object, _jwtTokenServiceMock.Object, _registerLoggerMock.Object);

        // Act & Assert
        await handler.Invoking(h => h.Handle(command, CancellationToken.None))
            .Should().ThrowAsync<ConflictException>();
    }

    [Fact]
    public async Task LoginUserHandler_ValidCredentials_ReturnsTokenAndUserInfo()
    {
        // Arrange
        var query = new LoginUserQuery { Email = "user@example.com", Password = "Password123" };
        _identityServiceMock.Setup(s => s.LoginAsync(query.Email, query.Password, It.IsAny<CancellationToken>()))
            .ReturnsAsync((true, "user-id-1", query.Email, new[] { "User" }));

        _jwtTokenServiceMock.Setup(j => j.CreateToken("user-id-1", query.Email, It.IsAny<IEnumerable<string>>()))
            .Returns("login-jwt-token");

        var handler = new LoginUserQueryHandler(_identityServiceMock.Object, _jwtTokenServiceMock.Object, _loginLoggerMock.Object);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Token.Should().Be("login-jwt-token");
        result.UserId.Should().Be("user-id-1");
        result.Email.Should().Be("user@example.com");
    }

    [Fact]
    public async Task LoginUserHandler_InvalidCredentials_ThrowsUnauthorizedException()
    {
        // Arrange
        var query = new LoginUserQuery { Email = "user@example.com", Password = "WrongPassword" };
        _identityServiceMock.Setup(s => s.LoginAsync(query.Email, query.Password, It.IsAny<CancellationToken>()))
            .ReturnsAsync((false, string.Empty, string.Empty, Array.Empty<string>()));

        var handler = new LoginUserQueryHandler(_identityServiceMock.Object, _jwtTokenServiceMock.Object, _loginLoggerMock.Object);

        // Act & Assert
        await handler.Invoking(h => h.Handle(query, CancellationToken.None))
            .Should().ThrowAsync<UnauthorizedException>();
    }
}
