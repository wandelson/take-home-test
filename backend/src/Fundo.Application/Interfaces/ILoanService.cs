using Fundo.Application.DTOs;

namespace Fundo.Application.Interfaces;

public interface ILoanService
{
    Task<IReadOnlyList<LoanResponse>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<LoanResponse?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<LoanResponse> CreateAsync(CreateLoanRequest request, CancellationToken cancellationToken = default);
    Task<LoanResponse> RecordPaymentAsync(
        Guid id,
        PaymentRequest request,
        string? idempotencyKey = null,
        CancellationToken cancellationToken = default);
}
