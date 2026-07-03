using FluentAssertions;
using Fundo.Application.Configuration;
using Fundo.Application.DTOs;
using Fundo.Infrastructure.Auth;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

namespace Fundo.Services.Tests.Unit;

public class JwtAuthServiceTests
{
    private static JwtAuthService CreateService(string? username = "admin", string? password = "admin123")
    {
        var authOptions = Options.Create(new AuthOptions { Username = username!, Password = password! });
        var jwtOptions = Options.Create(new JwtOptions
        {
            Issuer = "FundoLoanApi",
            Audience = "FundoLoanClient",
            Key = "SuperSecretDemoKeyThatIsAtLeast32CharactersLong!",
            ExpiresMinutes = 60
        });

        return new JwtAuthService(authOptions, jwtOptions, NullLogger<JwtAuthService>.Instance);
    }

    [Fact]
    public void Authenticate_WithValidCredentials_ReturnsToken()
    {
        var service = CreateService();

        var result = service.Authenticate(new LoginRequest { Username = "admin", Password = "admin123" });

        result.Should().NotBeNull();
        result!.Token.Should().NotBeNullOrWhiteSpace();
        result.ExpiresAt.Should().BeAfter(DateTimeOffset.UtcNow);
    }

    [Fact]
    public void Authenticate_WithInvalidCredentials_ReturnsNull()
    {
        var service = CreateService();

        var result = service.Authenticate(new LoginRequest { Username = "admin", Password = "wrong" });

        result.Should().BeNull();
    }
}
