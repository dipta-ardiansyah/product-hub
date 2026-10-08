using System.ComponentModel.DataAnnotations;
using MediatR;
using ProductHub.Application.Auth.Dtos;

namespace ProductHub.Application.Auth.Queries
{
    public class LoginUserQuery : IRequest<AuthResponseDto>
    {
        [Required, EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string Password { get; set; } = string.Empty;
    }
}