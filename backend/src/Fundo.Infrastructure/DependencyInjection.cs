using Fundo.Application.Configuration;
using Fundo.Application.Interfaces;
using Fundo.Infrastructure.Auth;
using Fundo.Infrastructure.Events;
using Fundo.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Fundo.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        ConfigureOptions(services, configuration);

        var database = configuration.GetSection(DatabaseOptions.SectionName).Get<DatabaseOptions>() ?? new DatabaseOptions();
        services.AddDbContext<LoanDbContext>(options =>
        {
            if (string.Equals(database.Provider, "InMemory", StringComparison.OrdinalIgnoreCase))
            {
                options.UseInMemoryDatabase(database.InMemoryName);
                return;
            }

            var connectionString = configuration.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException("ConnectionStrings:DefaultConnection is required for SqlServer.");

            options.UseSqlServer(connectionString);
        });

        services.AddScoped<ILoanRepository, LoanRepository>();
        services.AddScoped<IDomainEventDispatcher, LoggingDomainEventDispatcher>();
        services.AddSingleton<IAuthService, JwtAuthService>();
        return services;
    }

    public static IServiceCollection AddInfrastructureForTesting(
        this IServiceCollection services,
        Action<DbContextOptionsBuilder> configureDb)
    {
        ConfigureTestOptions(services);
        services.AddDbContext<LoanDbContext>(configureDb);
        services.AddScoped<ILoanRepository, LoanRepository>();
        services.AddScoped<IDomainEventDispatcher, LoggingDomainEventDispatcher>();
        services.AddSingleton<IAuthService, JwtAuthService>();
        return services;
    }

    public static IServiceCollection AddInfrastructureForTesting(this IServiceCollection services, string databaseName)
    {
        return services.AddInfrastructureForTesting(options => options.UseInMemoryDatabase(databaseName));
    }

    private static void ConfigureOptions(IServiceCollection services, IConfiguration configuration)
    {
        services.AddOptions<JwtOptions>()
            .Bind(configuration.GetSection(JwtOptions.SectionName))
            .Validate(o => !string.IsNullOrWhiteSpace(o.Key) && o.Key.Length >= 32, "Jwt:Key must be at least 32 characters.")
            .ValidateOnStart();

        services.AddOptions<AuthOptions>()
            .Bind(configuration.GetSection(AuthOptions.SectionName))
            .Validate(o => !string.IsNullOrWhiteSpace(o.Username) && !string.IsNullOrWhiteSpace(o.Password), "Auth credentials are required.")
            .ValidateOnStart();
    }

    private static void ConfigureTestOptions(IServiceCollection services)
    {
        services.AddOptions<JwtOptions>().Configure(o =>
        {
            o.Issuer = "FundoLoanApi";
            o.Audience = "FundoLoanClient";
            o.Key = "SuperSecretDemoKeyThatIsAtLeast32CharactersLong!";
            o.ExpiresMinutes = 60;
        });
        services.AddOptions<AuthOptions>().Configure(o =>
        {
            o.Username = "admin";
            o.Password = "admin123";
        });
    }
}
