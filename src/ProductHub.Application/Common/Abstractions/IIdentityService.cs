using ProductHub.Application.Auth.Dtos;

namespace ProductHub.Application.Common.Abstractions
{
    public interface IIdentityService
    {
        Task<(bool Succeeded, string UserId, IEnumerable<string> Errors)> RegisterAsync(
            string email, string password, CancellationToken cancellationToken);
        Task<(bool Succeeded, string UserId, string Email, IEnumerable<string> Roles)> LoginAsync(
            string email, string password, CancellationToken cancellationToken);
    }
}