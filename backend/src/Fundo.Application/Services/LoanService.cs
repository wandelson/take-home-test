using Fundo.Application.DTOs;
using Fundo.Application.Exceptions;
using Fundo.Application.Interfaces;
using Fundo.Application.Mapping;
using Fundo.Domain.Entities;
using Fundo.Domain.Exceptions;
using Microsoft.Extensions.Logging;

namespace Fundo.Application.Services;

public class LoanService : ILoanService
{
    private readonly ILoanRepository _repository;
    private readonly IDomainEventDispatcher _eventDispatcher;
    private readonly ILogger<LoanService> _logger;

    public LoanService(
        ILoanRepository repository,
        IDomainEventDispatcher eventDispatcher,
        ILogger<LoanService> logger)
    {
        _repository = repository;
        _eventDispatcher = eventDispatcher;
        _logger = logger;
    }

    public async Task<IReadOnlyList<LoanResponse>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var loans = await _repository.GetAllAsync(cancellationToken);
        return loans.Select(LoanMapper.ToResponse).ToList();
    }

    public async Task<LoanResponse?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var loan = await _repository.GetByIdAsync(id, cancellationToken);
        return loan is null ? null : LoanMapper.ToResponse(loan);
    }

    public async Task<LoanResponse> CreateAsync(CreateLoanRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            var loan = Loan.Create(request.Amount, request.ApplicantName);
            await _repository.AddAsync(loan, cancellationToken);
            await _repository.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Loan created {LoanId} for {ApplicantName}", loan.Id, loan.ApplicantName);
            return LoanMapper.ToResponse(loan);
        }
        catch (DomainException ex)
        {
            throw new DomainValidationException(ex.Message);
        }
    }

    public async Task<LoanResponse> RecordPaymentAsync(
        Guid id,
        PaymentRequest request,
        string? idempotencyKey = null,
        CancellationToken cancellationToken = default)
    {
        var normalizedKey = string.IsNullOrWhiteSpace(idempotencyKey) ? null : idempotencyKey.Trim();

        if (normalizedKey is not null)
        {
            var replay = await TryReplayIdempotentPaymentAsync(id, normalizedKey, cancellationToken);
            if (replay is not null)
            {
                return replay;
            }
        }

        var loan = await _repository.GetByIdForUpdateAsync(id, cancellationToken)
            ?? throw new NotFoundException($"Loan with id '{id}' was not found.");

        try
        {
            var domainEvent = loan.RecordPayment(request.Amount);

            await _repository.AddPaymentAsync(new Payment
            {
                Id = Guid.NewGuid(),
                LoanId = loan.Id,
                Amount = request.Amount,
                RecordedAt = domainEvent.OccurredAt,
                IdempotencyKey = normalizedKey
            }, cancellationToken);

            await _repository.SaveChangesAsync(cancellationToken);

            foreach (var pendingEvent in loan.DomainEvents)
            {
                await _eventDispatcher.DispatchAsync(pendingEvent, cancellationToken);
            }

            loan.ClearDomainEvents();
        }
        catch (IdempotencyConflictException)
        {
            if (normalizedKey is null)
            {
                throw;
            }

            var replay = await TryReplayIdempotentPaymentAsync(id, normalizedKey, cancellationToken);
            if (replay is not null)
            {
                return replay;
            }

            throw;
        }
        catch (DomainException ex)
        {
            throw new DomainValidationException(ex.Message);
        }

        _logger.LogInformation("Payment {Amount} recorded for loan {LoanId}", request.Amount, loan.Id);
        return LoanMapper.ToResponse(loan);
    }

    private async Task<LoanResponse?> TryReplayIdempotentPaymentAsync(
        Guid loanId,
        string idempotencyKey,
        CancellationToken cancellationToken)
    {
        var existingPayment = await _repository.FindPaymentByIdempotencyKeyAsync(loanId, idempotencyKey, cancellationToken);
        if (existingPayment is null)
        {
            return null;
        }

        var existingLoan = await _repository.GetByIdAsync(loanId, cancellationToken)
            ?? throw new NotFoundException($"Loan with id '{loanId}' was not found.");

        _logger.LogInformation(
            "Idempotent payment replay for loan {LoanId} with key {IdempotencyKey}",
            loanId,
            idempotencyKey);

        return LoanMapper.ToResponse(existingLoan);
    }
}
