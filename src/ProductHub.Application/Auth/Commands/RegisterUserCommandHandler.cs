using System.ComponentModel.DataAnnotations;
using MediatR;
using Microsoft.Extensions.Logging;
using ProductHub.Application.Auth.Dtos;
using ProductHub.Application.Common.Abstractions;
using ProductHub.Application.Common.Exceptions;

namespace ProductHub.Application.Auth.Commands
{
    public class RegisterUserCommandHandler : IRequestHandler<RegisterUserCommand, AuthResponseDto>
    {
        private readonly IIdentityService _identityService;
        private readonly IJwtTokenService _jwtTokenService;
        private readonly ILogger<RegisterUserCommandHandler> _logger;

        public RegisterUserCommandHandler(
            IIdentityService identityService,
            IJwtTokenService jwtTokenService,
            ILogger<RegisterUserCommandHandler> logger)
        {
            _identityService = identityService;
            _jwtTokenService = jwtTokenService;
            _logger = logger;
        }

        public async Task<AuthResponseDto> Handle(RegisterUserCommand request, CancellationToken cancellationToken)
        {
            var result = await _identityService.RegisterAsync(request.Email, request.Password, cancellationToken);

            if (!result.Succeeded)
            {
                var errors = result.Errors.ToList();
                if (errors.Any(e => e.Contains("already taken", StringComparison.OrdinalIgnoreCase) ||
                                    e.Contains("duplicate", StringComparison.OrdinalIgnoreCase)))
                {
                    throw new ConflictException(string.Join(", ", errors));
                }

                throw new RequestValidationException(new Dictionary<string, string[]>
                {
                    { "Identity", errors.ToArray() }
                });
            }

            var roles = new[] { "User" };
            var token = _jwtTokenService.CreateToken(result.UserId, request.Email, roles);

            _logger.LogInformation("User registered successfully");

            return new AuthResponseDto
            {
                Token = token,
                UserId = result.UserId,
                Email = request.Email
            };
        }
    }
}