namespace Fundo.Domain.Entities;

public class Payment
{
    public Guid Id { get; set; }
    public Guid LoanId { get; set; }
    public Loan Loan { get; set; } = null!;
    public decimal Amount { get; set; }
    public DateTimeOffset RecordedAt { get; set; }
    public string? IdempotencyKey { get; set; }
}
