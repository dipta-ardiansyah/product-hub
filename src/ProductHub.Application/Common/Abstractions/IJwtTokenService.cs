using ProductHub.Application.Auth.Dtos;

namespace ProductHub.Application.Common.Abstractions
{
    public interface IJwtTokenService
    {
        string CreateToken(string userId, string email, IEnumerable<string> roles);
    }
}