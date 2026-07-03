using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Fundo.Application.Configuration;
using Fundo.Application.DTOs;
using Fundo.Application.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace Fundo.Infrastructure.Auth;

public class JwtAuthService : IAuthService
{
    private readonly AuthOptions _authOptions;
    private readonly JwtOptions _jwtOptions;
    private readonly string _passwordHash;
    private readonly ILogger<JwtAuthService> _logger;

    public JwtAuthService(IOptions<AuthOptions> authOptions, IOptions<JwtOptions> jwtOptions, ILogger<JwtAuthService> logger)
    {
        _authOptions = authOptions.Value;
        _jwtOptions = jwtOptions.Value;
        _logger = logger;
        _passwordHash = BCrypt.Net.BCrypt.HashPassword(_authOptions.Password);
    }

    public AuthResult? Authenticate(LoginRequest request)
    {
        if (!string.Equals(request.Username, _authOptions.Username, StringComparison.Ordinal) ||
            !BCrypt.Net.BCrypt.Verify(request.Password, _passwordHash))
        {
            _logger.LogWarning("Login failed for user {Username}", request.Username);
            return null;
        }

        var expiresAt = DateTimeOffset.UtcNow.AddMinutes(_jwtOptions.ExpiresMinutes);
        var token = GenerateToken(request.Username, expiresAt.UtcDateTime);

        _logger.LogInformation("Login succeeded for user {Username}", request.Username);
        return new AuthResult(token, expiresAt);
    }

    private string GenerateToken(string username, DateTime expiresAt)
    {
        if (string.IsNullOrWhiteSpace(_jwtOptions.Key) || _jwtOptions.Key.Length < 32)
        {
            throw new InvalidOperationException("Jwt:Key must be at least 32 characters.");
        }

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtOptions.Key));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(ClaimTypes.Name, username),
            new Claim(JwtRegisteredClaimNames.Sub, username),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var token = new JwtSecurityToken(
            issuer: _jwtOptions.Issuer,
            audience: _jwtOptions.Audience,
            claims: claims,
            expires: expiresAt,
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
