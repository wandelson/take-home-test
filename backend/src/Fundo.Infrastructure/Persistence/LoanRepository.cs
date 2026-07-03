using Fundo.Application.Interfaces;
using Fundo.Domain.Entities;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace Fundo.Infrastructure.Persistence;

public class LoanRepository : ILoanRepository
{
    private readonly LoanDbContext _context;

    public LoanRepository(LoanDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<Loan>> GetAllAsync(CancellationToken cancellationToken = default) =>
        await _context.Loans
            .AsNoTracking()
            .OrderByDescending(l => l.CreatedAt)
            .ToListAsync(cancellationToken);

    public async Task<Loan?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        await _context.Loans
            .AsNoTracking()
            .FirstOrDefaultAsync(l => l.Id == id, cancellationToken);

    public async Task<Loan?> GetByIdForUpdateAsync(Guid id, CancellationToken cancellationToken = default) =>
        await _context.Loans.FirstOrDefaultAsync(l => l.Id == id, cancellationToken);

    public async Task<Payment?> FindPaymentByIdempotencyKeyAsync(
        Guid loanId,
        string idempotencyKey,
        CancellationToken cancellationToken = default) =>
        await _context.Payments
            .AsNoTracking()
            .FirstOrDefaultAsync(
                p => p.LoanId == loanId && p.IdempotencyKey == idempotencyKey,
                cancellationToken);

    public async Task AddAsync(Loan loan, CancellationToken cancellationToken = default) =>
        await _context.Loans.AddAsync(loan, cancellationToken);

    public async Task AddPaymentAsync(Payment payment, CancellationToken cancellationToken = default) =>
        await _context.Payments.AddAsync(payment, cancellationToken);

    public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            await _context.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException ex)
        {
            throw new Fundo.Application.Exceptions.ConcurrencyException(
                "The loan was modified by another request. Please retry.",
                ex);
        }
        catch (DbUpdateException ex) when (IsUniqueIdempotencyViolation(ex))
        {
            throw new Fundo.Application.Exceptions.IdempotencyConflictException(
                "A payment with this idempotency key was already recorded.",
                ex);
        }
    }

    private static bool IsUniqueIdempotencyViolation(DbUpdateException exception)
    {
        for (var inner = exception.InnerException; inner is not null; inner = inner.InnerException)
        {
            if (inner is SqlException { Number: 2601 or 2627 })
            {
                return true;
            }
        }

        return false;
    }
}
