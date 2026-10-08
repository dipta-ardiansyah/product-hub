using System.ComponentModel.DataAnnotations;
using MediatR;
using ProductHub.Application.Auth.Dtos;

namespace ProductHub.Application.Auth.Commands
{
    public class RegisterUserCommand : IRequest<AuthResponseDto>
    {
        [Required, EmailAddress, StringLength(256)]
        public string Email { get; set; } = string.Empty;

        [Required, StringLength(100, MinimumLength = 8, ErrorMessage = "Password must be at least 8 characters.")]
        public string Password { get; set; } = string.Empty;
    }
}