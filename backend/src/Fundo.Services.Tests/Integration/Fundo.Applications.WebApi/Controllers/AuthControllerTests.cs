using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Fundo.Application.DTOs;
using Microsoft.AspNetCore.Mvc.Testing;

namespace Fundo.Services.Tests.Integration.Fundo.Applications.WebApi.Controllers;

public class AuthControllerTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;
    private readonly CustomWebApplicationFactory _factory;

    public AuthControllerTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Login_WithValidCredentials_ShouldSetHttpOnlyCookie()
    {
        var response = await _client.PostAsJsonAsync("/auth/login", new LoginRequest
        {
            Username = "admin",
            Password = "admin123"
        });

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.Headers.TryGetValues("Set-Cookie", out var cookies).Should().BeTrue();
        cookies!.Should().Contain(c =>
            c.StartsWith("fundo_auth=", StringComparison.Ordinal) &&
            c.Contains("httponly", StringComparison.OrdinalIgnoreCase));

        var body = await response.Content.ReadFromJsonAsync<LoginResponse>();
        body.Should().NotBeNull();
        body!.ExpiresAt.Should().BeAfter(DateTimeOffset.UtcNow);
    }

    [Fact]
    public async Task Login_WithInvalidCredentials_ShouldReturn401()
    {
        var response = await _client.PostAsJsonAsync("/auth/login", new LoginRequest
        {
            Username = "admin",
            Password = "wrong"
        });

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Login_WithEmptyUsername_ShouldReturn400()
    {
        var response = await _client.PostAsJsonAsync("/auth/login", new LoginRequest
        {
            Username = "",
            Password = "admin123"
        });

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Login_WithEmptyPassword_ShouldReturn400()
    {
        var response = await _client.PostAsJsonAsync("/auth/login", new LoginRequest
        {
            Username = "admin",
            Password = ""
        });

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Login_WithWhitespaceCredentials_ShouldReturn400()
    {
        var response = await _client.PostAsJsonAsync("/auth/login", new LoginRequest
        {
            Username = "   ",
            Password = "   "
        });

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task GetLoans_WithoutToken_ShouldReturn401()
    {
        using var factory = new UnauthorizedWebApplicationFactory();
        using var client = factory.CreateClient();

        var response = await client.GetAsync("/loans");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Login_ThenAccessLoans_WithCookie_ShouldReturn200()
    {
        using var client = _factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            HandleCookies = true
        });

        var loginResponse = await client.PostAsJsonAsync("/auth/login", new LoginRequest
        {
            Username = "admin",
            Password = "admin123"
        });

        loginResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var loansResponse = await client.GetAsync("/loans");

        loansResponse.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}
