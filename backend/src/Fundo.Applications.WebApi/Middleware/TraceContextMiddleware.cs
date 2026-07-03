using Serilog.Context;

namespace Fundo.Applications.WebApi.Middleware;

public class TraceContextMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(HttpContext context)
    {
        using (LogContext.PushProperty("TraceId", context.TraceIdentifier))
        {
            await next(context);
        }
    }
}

public static class TraceContextMiddlewareExtensions
{
    public static IApplicationBuilder UseTraceContext(this IApplicationBuilder app) =>
        app.UseMiddleware<TraceContextMiddleware>();
}
