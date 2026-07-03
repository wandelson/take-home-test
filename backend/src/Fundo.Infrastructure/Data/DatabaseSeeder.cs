using Fundo.Domain.Entities;
using Fundo.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Fundo.Infrastructure.Data;

public static class DatabaseSeeder
{
    public static async Task SeedAsync(IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<LoanDbContext>();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<LoanDbContext>>();

        if (context.Database.IsSqlServer())
        {
            await context.Database.MigrateAsync();
        }
        else
        {
            await context.Database.EnsureCreatedAsync();
        }

        if (await context.Loans.AnyAsync())
        {
            return;
        }

        var seedLoans = new[]
        {
            new Loan
            {
                Id = Guid.Parse("11111111-1111-1111-1111-111111111111"),
                Amount = 1500.00m,
                CurrentBalance = 500.00m,
                ApplicantName = "Maria Silva",
                Status = LoanStatus.Active,
                CreatedAt = DateTimeOffset.UtcNow.AddMonths(-3)
            },
            new Loan
            {
                Id = Guid.Parse("22222222-2222-2222-2222-222222222222"),
                Amount = 25000.00m,
                CurrentBalance = 18750.00m,
                ApplicantName = "John Doe",
                Status = LoanStatus.Active,
                CreatedAt = DateTimeOffset.UtcNow.AddMonths(-6)
            },
            new Loan
            {
                Id = Guid.Parse("33333333-3333-3333-3333-333333333333"),
                Amount = 15000.00m,
                CurrentBalance = 0m,
                ApplicantName = "Jane Smith",
                Status = LoanStatus.Paid,
                CreatedAt = DateTimeOffset.UtcNow.AddMonths(-12)
            },
            new Loan
            {
                Id = Guid.Parse("44444444-4444-4444-4444-444444444444"),
                Amount = 50000.00m,
                CurrentBalance = 32500.00m,
                ApplicantName = "Robert Johnson",
                Status = LoanStatus.Active,
                CreatedAt = DateTimeOffset.UtcNow.AddMonths(-2)
            }
        };

        context.Loans.AddRange(seedLoans);
        await context.SaveChangesAsync();

        logger.LogInformation("Seeded {Count} loans into the database.", seedLoans.Length);
    }
}
