using ProductHub.Application.Auth.Dtos;

namespace ProductHub.Application.Common.Abstractions
{
    public interface IJwtTokenService
    {
        AuthResponseDto CreateToken(string userId, string email);
    }
}