using Fundo.Application.Interfaces;
using Fundo.Domain.Events;
using Microsoft.Extensions.Logging;

namespace Fundo.Infrastructure.Events;

public class LoggingDomainEventDispatcher : IDomainEventDispatcher
{
    private readonly ILogger<LoggingDomainEventDispatcher> _logger;

    public LoggingDomainEventDispatcher(ILogger<LoggingDomainEventDispatcher> logger)
    {
        _logger = logger;
    }

    public Task DispatchAsync(IDomainEvent domainEvent, CancellationToken cancellationToken = default)
    {
        if (domainEvent is PaymentRecordedEvent payment)
        {
            _logger.LogInformation(
                "Domain event {EventType} for loan {LoanId}: amount {Amount}, remaining {RemainingBalance}, paidInFull {PaidInFull}",
                nameof(PaymentRecordedEvent),
                payment.LoanId,
                payment.Amount,
                payment.RemainingBalance,
                payment.LoanPaidInFull);
        }
        else
        {
            _logger.LogInformation("Domain event {EventType} at {OccurredAt}", domainEvent.GetType().Name, domainEvent.OccurredAt);
        }

        return Task.CompletedTask;
    }
}
