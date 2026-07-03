using System.IdentityModel.Tokens.Jwt;
using Fundo.Application.Configuration;
using Fundo.Application.DTOs;
using Fundo.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Options;

namespace Fundo.Applications.WebApi.Controllers;

[ApiController]
[Route("auth")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly JwtOptions _jwtOptions;

    public AuthController(IAuthService authService, IOptions<JwtOptions> jwtOptions)
    {
        _authService = authService;
        _jwtOptions = jwtOptions.Value;
    }

    [HttpPost("login")]
    [AllowAnonymous]
    [EnableRateLimiting("login")]
    [ProducesResponseType(typeof(LoginResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    public ActionResult<LoginResponse> Login([FromBody] LoginRequest request)
    {
        var result = _authService.Authenticate(request);
        if (result is null)
        {
            return Unauthorized(new ProblemDetails
            {
                Title = "Unauthorized",
                Detail = "Invalid username or password.",
                Status = StatusCodes.Status401Unauthorized
            });
        }

        AppendAuthCookie(result.Token, result.ExpiresAt);
        return Ok(new LoginResponse(result.ExpiresAt));
    }

    [HttpPost("logout")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public IActionResult Logout()
    {
        DeleteAuthCookie();
        return NoContent();
    }

    [HttpGet("me")]
    [Authorize]
    [ProducesResponseType(typeof(SessionResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public ActionResult<SessionResponse> Me()
    {
        var expClaim = User.FindFirst(JwtRegisteredClaimNames.Exp)?.Value;
        var expiresAt = expClaim is not null
            ? DateTimeOffset.FromUnixTimeSeconds(long.Parse(expClaim))
            : DateTimeOffset.UtcNow;

        return Ok(new SessionResponse(expiresAt));
    }

    private void AppendAuthCookie(string token, DateTimeOffset expiresAt)
    {
        Response.Cookies.Append(_jwtOptions.CookieName, token, new CookieOptions
        {
            HttpOnly = true,
            Secure = Request.IsHttps,
            SameSite = SameSiteMode.Lax,
            Path = "/",
            Expires = expiresAt
        });
    }

    private void DeleteAuthCookie()
    {
        Response.Cookies.Delete(_jwtOptions.CookieName, new CookieOptions
        {
            HttpOnly = true,
            Secure = Request.IsHttps,
            SameSite = SameSiteMode.Lax,
            Path = "/"
        });
    }
}
