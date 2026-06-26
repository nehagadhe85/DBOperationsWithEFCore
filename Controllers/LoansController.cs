using Microsoft.AspNetCore.Mvc;
using DBOperationWithEFCore.DTOs;
using DBOperationWithEFCore.Services.Interfaces;

namespace DBOperationWithEFCore.Controllers;

[ApiController]
[Route("api/[controller]")]
public class LoansController : ControllerBase
{
    private readonly ILoanService _loanService;

    public LoansController(ILoanService loanService)
    {
        _loanService = loanService;
    }

    /// <summary>
    /// Get paginated list of loans with optional status filter.
    /// GET /api/loans?status=Active
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<PagedResult<BookLoanDto>>> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? status = null)
    {
        if (page < 1) page = 1;
        if (pageSize < 1 || pageSize > 50) pageSize = 10;

        var result = await _loanService.GetAllAsync(page, pageSize, status);
        return Ok(result);
    }

    /// <summary>
    /// Checkout a book for a member.
    /// POST /api/loans/checkout
    /// </summary>
    [HttpPost("checkout")]
    public async Task<ActionResult<BookLoanDto>> Checkout([FromBody] CreateLoanRequest request)
    {
        var loan = await _loanService.CheckoutBookAsync(request);
        return CreatedAtAction(nameof(GetAll), loan);
    }

    /// <summary>
    /// Return a borrowed book.
    /// PUT /api/loans/{id}/return
    /// </summary>
    [HttpPut("{id}/return")]
    public async Task<ActionResult<BookLoanDto>> Return(int id)
    {
        var loan = await _loanService.ReturnBookAsync(id);
        return Ok(loan);
    }

    /// <summary>
    /// Get all overdue loans.
    /// GET /api/loans/overdue
    /// </summary>
    [HttpGet("overdue")]
    public async Task<ActionResult<List<BookLoanDto>>> GetOverdue()
    {
        var loans = await _loanService.GetOverdueLoansAsync();
        return Ok(loans);
    }

    /// <summary>
    /// Get all loans for a specific member.
    /// GET /api/loans/member/5
    /// </summary>
    [HttpGet("member/{memberId}")]
    public async Task<ActionResult<List<BookLoanDto>>> GetMemberLoans(int memberId)
    {
        var loans = await _loanService.GetMemberLoansAsync(memberId);
        return Ok(loans);
    }

    /// <summary>
    /// Get dashboard statistics.
    /// GET /api/loans/dashboard
    /// </summary>
    [HttpGet("dashboard")]
    public async Task<ActionResult<DashboardStats>> GetDashboard()
    {
        var stats = await _loanService.GetDashboardStatsAsync();
        return Ok(stats);
    }
}
