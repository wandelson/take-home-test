using System.ComponentModel.DataAnnotations;

namespace Fundo.Application.DTOs;

public record PaymentRequest : IValidatableObject
{
    [Range(0.01, double.MaxValue, ErrorMessage = "Payment amount must be greater than zero.")]
    public decimal Amount { get; init; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (Amount <= 0)
        {
            yield return new ValidationResult(
                "Payment amount is required.",
                [nameof(Amount)]);
        }
    }
}
