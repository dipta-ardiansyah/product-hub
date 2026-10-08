using ProductHub.Application.Auth.Dtos;

namespace ProductHub.Application.Common.Abstractions
{
    public interface IIdentityService
    {
        Task<AuthResponseDto> RegisterAsync(
            string email, string password, CancellationToken cancellationToken);
        Task<AuthResponseDto> LoginAsync(
            string email, string password, CancellationToken cancellationToken);
    }
}