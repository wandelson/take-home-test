using FluentAssertions;
using Fundo.Domain.Entities;
using Fundo.Domain.Events;
using Fundo.Domain.Exceptions;

namespace Fundo.Services.Tests.Unit;

public class LoanDomainTests
{
    [Fact]
    public void RecordPayment_WhenFullyPaid_SetsStatusToPaid()
    {
        var loan = new Loan
        {
            Amount = 500m,
            CurrentBalance = 300m,
            Status = LoanStatus.Active
        };

        var domainEvent = loan.RecordPayment(300m);

        loan.CurrentBalance.Should().Be(0m);
        loan.Status.Should().Be(LoanStatus.Paid);
        domainEvent.LoanPaidInFull.Should().BeTrue();
        loan.DomainEvents.Should().ContainSingle(e => e is PaymentRecordedEvent);
    }

    [Fact]
    public void RecordPayment_WhenAlreadyPaid_ThrowsLoanAlreadyPaidException()
    {
        var loan = new Loan { Status = LoanStatus.Paid, CurrentBalance = 0m };

        var act = () => loan.RecordPayment(10m);

        act.Should().Throw<LoanAlreadyPaidException>();
    }

    [Fact]
    public void RecordPayment_WhenExceedsBalance_ThrowsPaymentExceedsBalanceException()
    {
        var loan = new Loan { Status = LoanStatus.Active, CurrentBalance = 100m };

        var act = () => loan.RecordPayment(200m);

        act.Should().Throw<PaymentExceedsBalanceException>();
    }

    [Fact]
    public void RecordPayment_WhenZeroAmount_ThrowsInvalidPaymentAmountException()
    {
        var loan = new Loan { Status = LoanStatus.Active, CurrentBalance = 100m };

        var act = () => loan.RecordPayment(0m);

        act.Should().Throw<InvalidPaymentAmountException>();
    }

    [Fact]
    public void Create_SetsBalanceEqualToAmount()
    {
        var loan = Loan.Create(1500m, "Maria Silva");

        loan.Amount.Should().Be(1500m);
        loan.CurrentBalance.Should().Be(1500m);
        loan.Status.Should().Be(LoanStatus.Active);
        loan.ApplicantName.Should().Be("Maria Silva");
    }

    [Fact]
    public void Create_WhenInvalidAmount_ThrowsInvalidLoanAmountException()
    {
        var act = () => Loan.Create(0m, "Test");

        act.Should().Throw<InvalidLoanAmountException>();
    }
}
