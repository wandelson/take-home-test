using FluentAssertions;
using Fundo.Application.Exceptions;
using Fundo.Applications.WebApi.Middleware;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging.Abstractions;

namespace Fundo.Services.Tests.Unit;

public class GlobalExceptionHandlerTests
{
    [Fact]
    public async Task TryHandleAsync_ConcurrencyException_Returns409()
    {
        var handler = new GlobalExceptionHandler(NullLogger<GlobalExceptionHandler>.Instance);
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();

        var handled = await handler.TryHandleAsync(
            context,
            new ConcurrencyException("Conflict"),
            CancellationToken.None);

        handled.Should().BeTrue();
        context.Response.StatusCode.Should().Be(StatusCodes.Status409Conflict);
    }
}
