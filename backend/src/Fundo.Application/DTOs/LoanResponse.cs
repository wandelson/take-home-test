namespace Fundo.Application.DTOs;

public record LoanResponse(
    Guid Id,
    decimal Amount,
    decimal CurrentBalance,
    string ApplicantName,
    string Status,
    DateTimeOffset CreatedAt);
