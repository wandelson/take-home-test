using Fundo.Application.DTOs;
using Fundo.Domain.Entities;

namespace Fundo.Application.Interfaces;

public interface ILoanRepository
{
    Task<IReadOnlyList<Loan>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<Loan?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Loan?> GetByIdForUpdateAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Payment?> FindPaymentByIdempotencyKeyAsync(Guid loanId, string idempotencyKey, CancellationToken cancellationToken = default);
    Task AddAsync(Loan loan, CancellationToken cancellationToken = default);
    Task AddPaymentAsync(Payment payment, CancellationToken cancellationToken = default);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
