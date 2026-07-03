using System.Text;
using System.Threading.RateLimiting;
using Fundo.Application;
using Fundo.Application.Configuration;
using Fundo.Infrastructure;
using Fundo.Infrastructure.Data;
using Fundo.Applications.WebApi.Middleware;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Serilog;
using Serilog.Events;
using Serilog.Formatting.Compact;

var builder = WebApplication.CreateBuilder(args);

var isTesting = builder.Environment.IsEnvironment("Testing");
const string consoleOutputTemplate =
    "[{Timestamp:HH:mm:ss} {Level:u3}] [{TraceId}] {Message:lj}{NewLine}{Exception}";

var loggerConfig = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .Enrich.WithProperty("Application", "FundoLoanApi")
    .Enrich.WithProperty("Environment", builder.Environment.EnvironmentName);

if (isTesting)
{
    loggerConfig.WriteTo.Console();
}
else if (builder.Environment.IsDevelopment())
{
    loggerConfig.WriteTo.Console(outputTemplate: consoleOutputTemplate);
}
else
{
    loggerConfig.WriteTo.Console(new CompactJsonFormatter());
}

Log.Logger = loggerConfig.CreateLogger();

builder.Host.UseSerilog();

if (!isTesting && builder.Configuration.GetValue("Observability:EnableTracing", builder.Environment.IsDevelopment()))
{
    builder.Services.AddOpenTelemetry()
        .ConfigureResource(resource => resource.AddService(
            builder.Configuration.GetValue<string>("Observability:ServiceName") ?? "FundoLoanApi"))
        .WithTracing(tracingBuilder =>
        {
            tracingBuilder
                .AddAspNetCoreInstrumentation()
                .AddHttpClientInstrumentation();

            if (builder.Configuration.GetValue("Observability:EnableConsoleExporter", false))
            {
                tracingBuilder.AddConsoleExporter();
            }
        });
}

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo { Title = "Loan Management API", Version = "v1" });
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header
    });
    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
            },
            Array.Empty<string>()
        }
    });
});

builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

var jwt = builder.Configuration.GetSection(JwtOptions.SectionName).Get<JwtOptions>()
    ?? throw new InvalidOperationException("Jwt configuration is required.");

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwt.Issuer,
            ValidAudience = jwt.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.Key))
        };
        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                if (context.Request.Cookies.TryGetValue(jwt.CookieName, out var token))
                {
                    context.Token = token;
                }

                return Task.CompletedTask;
            }
        };
    });
builder.Services.AddAuthorization();
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    options.AddPolicy("login", httpContext =>
        RateLimitPartition.GetFixedWindowLimiter(
            httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 10,
                Window = TimeSpan.FromMinutes(1)
            }));
});
builder.Services.AddHealthChecks()
    .AddDbContextCheck<Fundo.Infrastructure.Persistence.LoanDbContext>("database");

builder.Services.AddCors(options =>
{
    options.AddPolicy("Frontend", policy =>
        policy.WithOrigins(
                builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? ["http://localhost:4200"])
            .AllowCredentials()
            .WithHeaders("Authorization", "Content-Type", "Idempotency-Key")
            .WithMethods("GET", "POST", "OPTIONS"));
});

var app = builder.Build();

app.UseSerilogRequestLogging(options =>
{
    options.GetLevel = (httpContext, _, ex) =>
    {
        if (ex is not null)
        {
            return LogEventLevel.Error;
        }

        return httpContext.Request.Path.StartsWithSegments("/health")
            ? LogEventLevel.Verbose
            : LogEventLevel.Information;
    };

    options.EnrichDiagnosticContext = (diagnosticContext, httpContext) =>
    {
        diagnosticContext.Set("TraceId", httpContext.TraceIdentifier);
        diagnosticContext.Set("User", httpContext.User.Identity?.Name ?? "anonymous");
    };
});
app.UseExceptionHandler();
app.UseTraceContext();
app.UseRateLimiter();
app.UseCors("Frontend");
app.UseAuthentication();
app.UseAuthorization();

if (builder.Configuration.GetValue("EnableSwagger", app.Environment.IsDevelopment()))
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapControllers();
app.MapHealthChecks("/health").AllowAnonymous();
await DatabaseSeeder.SeedAsync(app.Services);

app.Run();

public partial class Program;
