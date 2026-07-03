using Fundo.Application.DTOs;
using Fundo.Application.Exceptions;
using Fundo.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Fundo.Applications.WebApi.Controllers;

[ApiController]
[Authorize]
[Route("loans")]
public class LoanManagementController : ControllerBase
{
    private readonly ILoanService _loanService;

    public LoanManagementController(ILoanService loanService)
    {
        _loanService = loanService;
    }

    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<LoanResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<LoanResponse>>> GetAll(CancellationToken cancellationToken)
    {
        return Ok(await _loanService.GetAllAsync(cancellationToken));
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(LoanResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<LoanResponse>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var loan = await _loanService.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException($"Loan with id '{id}' was not found.");

        return Ok(loan);
    }

    [HttpPost]
    [ProducesResponseType(typeof(LoanResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<LoanResponse>> Create([FromBody] CreateLoanRequest request, CancellationToken cancellationToken)
    {
        var loan = await _loanService.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = loan.Id }, loan);
    }

    [HttpPost("{id:guid}/payment")]
    [ProducesResponseType(typeof(LoanResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<LoanResponse>> RecordPayment(
        Guid id,
        [FromBody] PaymentRequest request,
        [FromHeader(Name = "Idempotency-Key")] string? idempotencyKey,
        CancellationToken cancellationToken)
    {
        return Ok(await _loanService.RecordPaymentAsync(id, request, idempotencyKey, cancellationToken));
    }
}
