using System.ComponentModel.DataAnnotations;

namespace Fundo.Application.DTOs;

public record CreateLoanRequest : IValidatableObject
{
    [Range(0.01, double.MaxValue, ErrorMessage = "Loan amount must be greater than zero.")]
    public decimal Amount { get; init; }

    [Required(ErrorMessage = "Applicant name is required.")]
    [MaxLength(200, ErrorMessage = "Applicant name cannot exceed 200 characters.")]
    public string ApplicantName { get; init; } = string.Empty;

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (Amount <= 0)
        {
            yield return new ValidationResult(
                "Loan amount is required.",
                [nameof(Amount)]);
        }

        if (string.IsNullOrWhiteSpace(ApplicantName))
        {
            yield return new ValidationResult(
                "Applicant name is required.",
                [nameof(ApplicantName)]);
        }
    }
}
