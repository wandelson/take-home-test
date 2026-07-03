namespace Fundo.Application.DTOs;

public record AuthResult(string Token, DateTimeOffset ExpiresAt);
