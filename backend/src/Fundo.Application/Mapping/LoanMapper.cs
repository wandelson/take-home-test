using Fundo.Application.DTOs;
using Fundo.Domain.Entities;

namespace Fundo.Application.Mapping;

public static class LoanMapper
{
    public static LoanResponse ToResponse(Loan loan) =>
        new(
            loan.Id,
            loan.Amount,
            loan.CurrentBalance,
            loan.ApplicantName,
            loan.Status == LoanStatus.Paid ? "paid" : "active",
            loan.CreatedAt);
}
