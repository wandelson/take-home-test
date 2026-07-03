using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Fundo.Application.DTOs;
using Fundo.Domain.Entities;
using Fundo.Services.Tests.Integration;

namespace Fundo.Services.Tests.Integration.Fundo.Applications.WebApi.Controllers;

public class LoanManagementControllerTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;
    private readonly CustomWebApplicationFactory _factory;

    public LoanManagementControllerTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task HealthCheck_ShouldReturnHealthy()
    {
        var response = await _client.GetAsync("/health");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetLoans_ShouldReturnSeededLoans()
    {
        var response = await _client.GetAsync("/loans");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var loans = await response.Content.ReadFromJsonAsync<List<LoanResponse>>();
        loans.Should().NotBeNull();
        loans!.Count.Should().BeGreaterThanOrEqualTo(4);
        loans.Should().Contain(l => l.ApplicantName == "Maria Silva");
    }

    [Fact]
    public async Task CreateLoan_ShouldReturnCreatedLoan()
    {
        var request = new CreateLoanRequest { Amount = 1500m, ApplicantName = "Maria Silva" };

        var response = await _client.PostAsJsonAsync("/loans", request);

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var loan = await response.Content.ReadFromJsonAsync<LoanResponse>();
        loan.Should().NotBeNull();
        loan!.Amount.Should().Be(1500m);
        loan.CurrentBalance.Should().Be(1500m);
        loan.ApplicantName.Should().Be("Maria Silva");
        loan.Status.Should().Be("active");
    }

    [Fact]
    public async Task CreateLoan_WithEmptyApplicantName_ShouldReturn400()
    {
        var request = new CreateLoanRequest { Amount = 1500m, ApplicantName = "" };

        var response = await _client.PostAsJsonAsync("/loans", request);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task CreateLoan_WithInvalidAmount_ShouldReturn400()
    {
        var request = new CreateLoanRequest { Amount = 0m, ApplicantName = "Test User" };

        var response = await _client.PostAsJsonAsync("/loans", request);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task GetLoanById_WhenExists_ShouldReturnLoan()
    {
        var loanId = Guid.NewGuid();
        await _factory.SeedLoanAsync(new Loan
        {
            Id = loanId,
            Amount = 2000m,
            CurrentBalance = 1500m,
            ApplicantName = "Test User",
            Status = LoanStatus.Active,
            CreatedAt = DateTimeOffset.UtcNow
        });

        var response = await _client.GetAsync($"/loans/{loanId}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var loan = await response.Content.ReadFromJsonAsync<LoanResponse>();
        loan!.ApplicantName.Should().Be("Test User");
    }

    [Fact]
    public async Task GetLoanById_WhenNotFound_ShouldReturn404()
    {
        var response = await _client.GetAsync($"/loans/{Guid.NewGuid()}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task RecordPayment_ShouldReduceBalance()
    {
        var loanId = Guid.NewGuid();
        await _factory.SeedLoanAsync(new Loan
        {
            Id = loanId,
            Amount = 1000m,
            CurrentBalance = 500m,
            ApplicantName = "Payment Test",
            Status = LoanStatus.Active,
            CreatedAt = DateTimeOffset.UtcNow
        });

        var response = await _client.PostAsJsonAsync($"/loans/{loanId}/payment", new PaymentRequest { Amount = 200m });

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var loan = await response.Content.ReadFromJsonAsync<LoanResponse>();
        loan!.CurrentBalance.Should().Be(300m);
        loan.Status.Should().Be("active");
    }

    [Fact]
    public async Task RecordPayment_WhenFullyPaid_ShouldSetStatusToPaid()
    {
        var loanId = Guid.NewGuid();
        await _factory.SeedLoanAsync(new Loan
        {
            Id = loanId,
            Amount = 500m,
            CurrentBalance = 300m,
            ApplicantName = "Full Payment Test",
            Status = LoanStatus.Active,
            CreatedAt = DateTimeOffset.UtcNow
        });

        var response = await _client.PostAsJsonAsync($"/loans/{loanId}/payment", new PaymentRequest { Amount = 300m });

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var loan = await response.Content.ReadFromJsonAsync<LoanResponse>();
        loan!.CurrentBalance.Should().Be(0m);
        loan.Status.Should().Be("paid");
    }

    [Fact]
    public async Task RecordPayment_WhenExceedsBalance_ShouldReturn400()
    {
        var loanId = Guid.NewGuid();
        await _factory.SeedLoanAsync(new Loan
        {
            Id = loanId,
            Amount = 500m,
            CurrentBalance = 100m,
            ApplicantName = "Overpayment Test",
            Status = LoanStatus.Active,
            CreatedAt = DateTimeOffset.UtcNow
        });

        var response = await _client.PostAsJsonAsync($"/loans/{loanId}/payment", new PaymentRequest { Amount = 200m });

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task RecordPayment_WhenLoanAlreadyPaid_ShouldReturn400()
    {
        var loanId = Guid.NewGuid();
        await _factory.SeedLoanAsync(new Loan
        {
            Id = loanId,
            Amount = 500m,
            CurrentBalance = 0m,
            ApplicantName = "Paid Loan Test",
            Status = LoanStatus.Paid,
            CreatedAt = DateTimeOffset.UtcNow
        });

        var response = await _client.PostAsJsonAsync($"/loans/{loanId}/payment", new PaymentRequest { Amount = 10m });

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task RecordPayment_WithZeroAmount_ShouldReturn400()
    {
        var loanId = Guid.NewGuid();
        await _factory.SeedLoanAsync(new Loan
        {
            Id = loanId,
            Amount = 500m,
            CurrentBalance = 100m,
            ApplicantName = "Zero Payment Test",
            Status = LoanStatus.Active,
            CreatedAt = DateTimeOffset.UtcNow
        });

        var response = await _client.PostAsJsonAsync($"/loans/{loanId}/payment", new PaymentRequest { Amount = 0m });

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task CreateLoan_WithWhitespaceApplicantName_ShouldReturn400()
    {
        var response = await _client.PostAsJsonAsync("/loans", new CreateLoanRequest
        {
            Amount = 100m,
            ApplicantName = "   "
        });

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task RecordPayment_WhenLoanNotFound_ShouldReturn404()
    {
        var response = await _client.PostAsJsonAsync(
            $"/loans/{Guid.NewGuid()}/payment",
            new PaymentRequest { Amount = 10m });

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task RecordPayment_WithIdempotencyKey_ShouldNotDoubleCharge()
    {
        var loanId = Guid.NewGuid();
        await _factory.SeedLoanAsync(new Loan
        {
            Id = loanId,
            Amount = 500m,
            CurrentBalance = 500m,
            ApplicantName = "Idempotency Integration",
            Status = LoanStatus.Active,
            CreatedAt = DateTimeOffset.UtcNow
        });

        async Task<HttpResponseMessage> PayAsync()
        {
            var request = new HttpRequestMessage(HttpMethod.Post, $"/loans/{loanId}/payment")
            {
                Content = JsonContent.Create(new PaymentRequest { Amount = 100m })
            };
            request.Headers.Add("Idempotency-Key", "integration-key-001");
            return await _client.SendAsync(request);
        }

        var first = await PayAsync();
        var second = await PayAsync();

        first.StatusCode.Should().Be(HttpStatusCode.OK);
        second.StatusCode.Should().Be(HttpStatusCode.OK);

        var loan = await first.Content.ReadFromJsonAsync<LoanResponse>();
        loan!.CurrentBalance.Should().Be(400m);
    }
}
