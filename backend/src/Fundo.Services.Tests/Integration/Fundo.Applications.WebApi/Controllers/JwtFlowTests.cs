using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using FluentAssertions;
using Fundo.Application.DTOs;

namespace Fundo.Services.Tests.Integration.Fundo.Applications.WebApi.Controllers;

public class JwtFlowTests : IClassFixture<JwtWebApplicationFactory>
{
    private readonly HttpClient _client;

    public JwtFlowTests(JwtWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Login_ThenAccessLoans_WithBearerToken_ShouldReturn200()
    {
        var loginResponse = await _client.PostAsJsonAsync("/auth/login", new LoginRequest
        {
            Username = "admin",
            Password = "admin123"
        });

        loginResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var token = ExtractAuthToken(loginResponse);
        token.Should().NotBeNullOrWhiteSpace();

        var request = new HttpRequestMessage(HttpMethod.Get, "/loans");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var loansResponse = await _client.SendAsync(request);

        loansResponse.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    private static string? ExtractAuthToken(HttpResponseMessage loginResponse)
    {
        if (!loginResponse.Headers.TryGetValues("Set-Cookie", out var cookies))
        {
            return null;
        }

        var authCookie = cookies.FirstOrDefault(c => c.StartsWith("fundo_auth=", StringComparison.Ordinal));
        if (authCookie is null)
        {
            return null;
        }

        var value = authCookie.Split(';')[0]["fundo_auth=".Length..];
        return Uri.UnescapeDataString(value);
    }
}
