using Fundo.Domain.Events;

namespace Fundo.Domain.Events;

public sealed record PaymentRecordedEvent(
    Guid LoanId,
    decimal Amount,
    decimal RemainingBalance,
    bool LoanPaidInFull,
    DateTimeOffset OccurredAt) : IDomainEvent;
