using MediatR;
using Microsoft.Extensions.Logging;
using ProductHub.Application.Auth.Dtos;
using ProductHub.Application.Common.Abstractions;
using ProductHub.Application.Common.Exceptions;

namespace ProductHub.Application.Auth.Queries
{
    public class LoginUserQueryHandler : IRequestHandler<LoginUserQuery, AuthResponseDto>
    {
        private readonly IIdentityService _identityService;
        private readonly IJwtTokenService _jwtTokenService;
        private readonly ILogger<LoginUserQueryHandler> _logger;

        public LoginUserQueryHandler(
            IIdentityService identityService,
            IJwtTokenService jwtTokenService,
            ILogger<LoginUserQueryHandler> logger)
        {
            _identityService = identityService;
            _jwtTokenService = jwtTokenService;
            _logger = logger;
        }

        public async Task<AuthResponseDto> Handle(LoginUserQuery request, CancellationToken cancellationToken)
        {
            var result = await _identityService.LoginAsync(request.Email, request.Password, cancellationToken);

            if (!result.Succeeded)
            {
                _logger.LogWarning("Login failed for email {Email}", request.Email);
                throw new UnauthorizedException("Invalid email or password.");
            }

            var token = _jwtTokenService.CreateToken(result.UserId, result.Email, result.Roles);

            _logger.LogInformation("User logged in successfully");

            return new AuthResponseDto
            {
                Token = token,
                UserId = result.UserId,
                Email = result.Email
            };
        }
    }
}