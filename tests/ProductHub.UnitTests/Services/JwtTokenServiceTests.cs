using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using FluentAssertions;
using Microsoft.Extensions.Options;
using ProductHub.Infrastructure.Identity;
using Xunit;

namespace ProductHub.UnitTests.Services;

public class JwtTokenServiceTests
{
    [Fact]
    public void GenerateToken_ValidInputs_GeneratesValidJwtWithClaims()
    {
        // Arrange
        var settings = new JwtSettings
        {
            SecretKey = "SuperSecretKeyForTestingPurposeMustBeAtLeast32Bytes!",
            Issuer = "ProductHubTest",
            Audience = "ProductHubUsersTest",
            ExpiryMinutes = 30
        };

        var options = Options.Create(settings);
        var service = new JwtTokenService(options);

        // Act
        var token = service.CreateToken("user-123", "user@example.com", new[] { "User", "Admin" });

        // Assert
        token.Should().NotBeNullOrWhiteSpace();

        var handler = new JwtSecurityTokenHandler();
        var jwt = handler.ReadJwtToken(token);

        jwt.Issuer.Should().Be("ProductHubTest");
        jwt.Audiences.Should().Contain("ProductHubUsersTest");
        jwt.Claims.Should().Contain(c => c.Type == JwtRegisteredClaimNames.Sub && c.Value == "user-123");
        jwt.Claims.Should().Contain(c => c.Type == JwtRegisteredClaimNames.Email && c.Value == "user@example.com");
        jwt.Claims.Where(c => c.Type == "role" || c.Type == ClaimTypes.Role).Select(c => c.Value).Should().Contain(new[] { "User", "Admin" });
    }
}
