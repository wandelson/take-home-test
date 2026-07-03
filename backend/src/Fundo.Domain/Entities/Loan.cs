using Fundo.Domain.Events;
using Fundo.Domain.Exceptions;

namespace Fundo.Domain.Entities;

public class Loan
{
    private readonly List<IDomainEvent> _domainEvents = [];

    public Guid Id { get; set; }
    public decimal Amount { get; set; }
    public decimal CurrentBalance { get; set; }
    public string ApplicantName { get; set; } = string.Empty;
    public LoanStatus Status { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public byte[] RowVersion { get; set; } = [];
    public ICollection<Payment> Payments { get; set; } = [];

    public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents;

    public void ClearDomainEvents() => _domainEvents.Clear();

    public static Loan Create(decimal amount, string applicantName)
    {
        if (amount <= 0)
        {
            throw new InvalidLoanAmountException();
        }

        if (string.IsNullOrWhiteSpace(applicantName))
        {
            throw new InvalidApplicantNameException();
        }

        return new Loan
        {
            Id = Guid.NewGuid(),
            Amount = amount,
            CurrentBalance = amount,
            ApplicantName = applicantName.Trim(),
            Status = LoanStatus.Active,
            CreatedAt = DateTimeOffset.UtcNow
        };
    }

    public PaymentRecordedEvent RecordPayment(decimal amount)
    {
        if (amount <= 0)
        {
            throw new InvalidPaymentAmountException();
        }

        if (Status == LoanStatus.Paid)
        {
            throw new LoanAlreadyPaidException();
        }

        if (amount > CurrentBalance)
        {
            throw new PaymentExceedsBalanceException();
        }

        CurrentBalance -= amount;
        if (CurrentBalance == 0)
        {
            Status = LoanStatus.Paid;
        }

        var domainEvent = new PaymentRecordedEvent(
            Id,
            amount,
            CurrentBalance,
            Status == LoanStatus.Paid,
            DateTimeOffset.UtcNow);

        _domainEvents.Add(domainEvent);
        return domainEvent;
    }
}
