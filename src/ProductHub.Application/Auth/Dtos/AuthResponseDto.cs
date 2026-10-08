namespace ProductHub.Application.Auth.Dtos
{
    public record AuthResponseDto(string Token, DateTime ExpiresAtUtc, string Email);
}