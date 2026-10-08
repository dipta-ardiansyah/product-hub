namespace ProductHub.Application.Auth
{
    public record AuthResponse(string Token, DateTime ExpiresAtUtc, string Email);
}