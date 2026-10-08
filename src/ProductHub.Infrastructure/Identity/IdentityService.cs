using Microsoft.AspNetCore.Identity;
using ProductHub.Application.Common.Abstractions;

namespace ProductHub.Infrastructure.Identity
{
    public class IdentityService : IIdentityService
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public IdentityService(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        public async Task<(bool Succeeded, string UserId, IEnumerable<string> Errors)> RegisterAsync(
            string email,
            string password,
            CancellationToken cancellationToken = default)
        {
            var existingUser = await _userManager.FindByEmailAsync(email);
            if (existingUser != null)
            {
                return (false, string.Empty, new[] { "An account with this email already exists." });
            }

            var user = new ApplicationUser
            {
                UserName = email,
                Email = email
            };

            var result = await _userManager.CreateAsync(user, password);
            if (!result.Succeeded)
            {
                return (false, string.Empty, result.Errors.Select(e => e.Description));
            }

            return (true, user.Id, Array.Empty<string>());
        }

        public async Task<(bool Succeeded, string UserId, string Email, IEnumerable<string> Roles)> LoginAsync(
            string email,
            string password,
            CancellationToken cancellationToken = default)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
            {
                return (false, string.Empty, string.Empty, Array.Empty<string>());
            }

            var isValid = await _userManager.CheckPasswordAsync(user, password);
            if (!isValid)
            {
                return (false, string.Empty, string.Empty, Array.Empty<string>());
            }

            var roles = await _userManager.GetRolesAsync(user);
            if (!roles.Any())
            {
                roles = new List<string> { "User" };
            }

            return (true, user.Id, user.Email ?? email, roles);
        }
    }
}