using Fundo.Infrastructure;
using Fundo.Infrastructure.Persistence;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Fundo.Services.Tests.Integration;

public class UnauthorizedWebApplicationFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.ConfigureServices(services =>
        {
            services.RemoveAll(typeof(DbContextOptions<LoanDbContext>));
            services.RemoveAll(typeof(LoanDbContext));
            services.AddInfrastructureForTesting($"Unauthorized_{Guid.NewGuid()}");
        });
    }
}
