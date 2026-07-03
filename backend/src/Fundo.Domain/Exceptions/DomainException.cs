namespace Fundo.Domain.Exceptions;

public abstract class DomainException(string message) : Exception(message);

public sealed class LoanAlreadyPaidException()
    : DomainException("Cannot record payment on a loan that is already paid.");

public sealed class PaymentExceedsBalanceException()
    : DomainException("Payment amount cannot exceed the current balance.");

public sealed class InvalidPaymentAmountException()
    : DomainException("Payment amount must be greater than zero.");

public sealed class InvalidLoanAmountException()
    : DomainException("Loan amount must be greater than zero.");

public sealed class InvalidApplicantNameException()
    : DomainException("Applicant name is required.");
