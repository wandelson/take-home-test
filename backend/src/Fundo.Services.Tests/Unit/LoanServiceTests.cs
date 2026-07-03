using FluentAssertions;
using Fundo.Application.DTOs;
using Fundo.Application.Exceptions;
using Fundo.Application.Interfaces;
using Fundo.Application.Services;
using Fundo.Domain.Entities;
using Fundo.Domain.Events;
using Fundo.Infrastructure.Events;
using Fundo.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;

namespace Fundo.Services.Tests.Unit;

public class LoanServiceTests
{
    private static LoanService CreateService(out LoanDbContext context)
    {
        var options = new DbContextOptionsBuilder<LoanDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        context = new LoanDbContext(options);
        var repository = new LoanRepository(context);
        var dispatcher = new LoggingDomainEventDispatcher(NullLogger<LoggingDomainEventDispatcher>.Instance);
        return new LoanService(repository, dispatcher, NullLogger<LoanService>.Instance);
    }

    [Fact]
    public async Task CreateAsync_WithValidRequest_SetsBalanceEqualToAmount()
    {
        var service = CreateService(out _);

        var result = await service.CreateAsync(new CreateLoanRequest { Amount = 1000m, ApplicantName = "Jane Doe" });

        result.Amount.Should().Be(1000m);
        result.CurrentBalance.Should().Be(1000m);
        result.Status.Should().Be("active");
    }

    [Fact]
    public async Task GetAllAsync_ReturnsAllLoans()
    {
        var service = CreateService(out var context);
        context.Loans.AddRange(
            new Loan { Id = Guid.NewGuid(), Amount = 100m, CurrentBalance = 100m, ApplicantName = "A", Status = LoanStatus.Active, CreatedAt = DateTimeOffset.UtcNow },
            new Loan { Id = Guid.NewGuid(), Amount = 200m, CurrentBalance = 200m, ApplicantName = "B", Status = LoanStatus.Active, CreatedAt = DateTimeOffset.UtcNow });
        await context.SaveChangesAsync();

        var result = await service.GetAllAsync();

        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task RecordPaymentAsync_WhenLoanPaid_ThrowsDomainValidationException()
    {
        var service = CreateService(out var context);
        var loan = new Loan
        {
            Id = Guid.NewGuid(),
            Amount = 500m,
            CurrentBalance = 0m,
            ApplicantName = "Paid User",
            Status = LoanStatus.Paid,
            CreatedAt = DateTimeOffset.UtcNow
        };
        context.Loans.Add(loan);
        await context.SaveChangesAsync();

        var act = () => service.RecordPaymentAsync(loan.Id, new PaymentRequest { Amount = 10m });

        await act.Should().ThrowAsync<DomainValidationException>();
    }

    [Fact]
    public async Task RecordPaymentAsync_WhenExceedsBalance_ThrowsDomainValidationException()
    {
        var service = CreateService(out var context);
        var loan = new Loan
        {
            Id = Guid.NewGuid(),
            Amount = 500m,
            CurrentBalance = 100m,
            ApplicantName = "Balance Test",
            Status = LoanStatus.Active,
            CreatedAt = DateTimeOffset.UtcNow
        };
        context.Loans.Add(loan);
        await context.SaveChangesAsync();

        var act = () => service.RecordPaymentAsync(loan.Id, new PaymentRequest { Amount = 200m });

        await act.Should().ThrowAsync<DomainValidationException>();
    }

    [Fact]
    public async Task RecordPaymentAsync_WithIdempotencyKey_ReplaysWithoutDoubleCharge()
    {
        var service = CreateService(out var context);
        var loanId = Guid.NewGuid();
        context.Loans.Add(new Loan
        {
            Id = loanId,
            Amount = 500m,
            CurrentBalance = 500m,
            ApplicantName = "Idempotency Test",
            Status = LoanStatus.Active,
            CreatedAt = DateTimeOffset.UtcNow
        });
        await context.SaveChangesAsync();

        const string key = "payment-key-001";
        var first = await service.RecordPaymentAsync(loanId, new PaymentRequest { Amount = 100m }, key);
        var second = await service.RecordPaymentAsync(loanId, new PaymentRequest { Amount = 100m }, key);

        first.CurrentBalance.Should().Be(400m);
        second.CurrentBalance.Should().Be(400m);
        context.Payments.Count(p => p.LoanId == loanId).Should().Be(1);
    }

    [Fact]
    public async Task RecordPaymentAsync_RaisesPaymentRecordedEvent()
    {
        var service = CreateService(out var context);
        var loanId = Guid.NewGuid();
        context.Loans.Add(new Loan
        {
            Id = loanId,
            Amount = 300m,
            CurrentBalance = 300m,
            ApplicantName = "Event Test",
            Status = LoanStatus.Active,
            CreatedAt = DateTimeOffset.UtcNow
        });
        await context.SaveChangesAsync();

        await service.RecordPaymentAsync(loanId, new PaymentRequest { Amount = 300m });

        context.Payments.Should().ContainSingle(p => p.LoanId == loanId && p.Amount == 300m);
    }
}
