using Fundo.Infrastructure;
using Fundo.Infrastructure.Persistence;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Fundo.Services.Tests.Integration;

/// <summary>
/// Uses real JWT authentication (no TestAuthHandler) for end-to-end auth flow tests.
/// </summary>
public class JwtWebApplicationFactory : WebApplicationFactory<Program>
{
    private readonly string _databaseName = $"FundoJwt_{Guid.NewGuid()}";

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.ConfigureServices(services =>
        {
            services.RemoveAll(typeof(DbContextOptions<LoanDbContext>));
            services.RemoveAll(typeof(LoanDbContext));

            services.AddInfrastructureForTesting(_databaseName);
        });
    }
}
