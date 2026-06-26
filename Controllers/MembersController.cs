using Microsoft.AspNetCore.Mvc;
using DBOperationWithEFCore.DTOs;
using DBOperationWithEFCore.Services.Interfaces;

namespace DBOperationWithEFCore.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MembersController : ControllerBase
{
    private readonly IMemberService _memberService;

    public MembersController(IMemberService memberService)
    {
        _memberService = memberService;
    }

    [HttpGet]
    public async Task<ActionResult<PagedResult<MemberDto>>> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? search = null)
    {
        if (page < 1) page = 1;
        if (pageSize < 1 || pageSize > 50) pageSize = 10;

        var result = await _memberService.GetAllAsync(page, pageSize, search);
        return Ok(result);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<MemberDto>> GetById(int id)
    {
        var member = await _memberService.GetByIdAsync(id);
        if (member == null) return NotFound(new { message = $"Member with ID {id} not found." });
        return Ok(member);
    }

    [HttpPost]
    public async Task<ActionResult<MemberDto>> Create([FromBody] CreateMemberRequest request)
    {
        var member = await _memberService.CreateAsync(request);
        return CreatedAtAction(nameof(GetById), new { id = member.Id }, member);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<MemberDto>> Update(int id, [FromBody] UpdateMemberRequest request)
    {
        var member = await _memberService.UpdateAsync(id, request);
        if (member == null) return NotFound(new { message = $"Member with ID {id} not found." });
        return Ok(member);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await _memberService.DeleteAsync(id);
        if (!result) return NotFound(new { message = $"Member with ID {id} not found." });
        return NoContent();
    }
}
