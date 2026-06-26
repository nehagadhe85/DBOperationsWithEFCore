using Microsoft.AspNetCore.Mvc;
using DBOperationWithEFCore.DTOs;
using DBOperationWithEFCore.Services.Interfaces;

namespace DBOperationWithEFCore.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthorsController : ControllerBase
{
    private readonly IAuthorService _authorService;

    public AuthorsController(IAuthorService authorService)
    {
        _authorService = authorService;
    }

    [HttpGet]
    public async Task<ActionResult<List<AuthorDto>>> GetAll()
    {
        var authors = await _authorService.GetAllAsync();
        return Ok(authors);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<AuthorDto>> GetById(int id)
    {
        var author = await _authorService.GetByIdAsync(id);
        if (author == null) return NotFound(new { message = $"Author with ID {id} not found." });
        return Ok(author);
    }

    [HttpPost]
    public async Task<ActionResult<AuthorDto>> Create([FromBody] CreateAuthorRequest request)
    {
        var author = await _authorService.CreateAsync(request);
        return CreatedAtAction(nameof(GetById), new { id = author.Id }, author);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<AuthorDto>> Update(int id, [FromBody] UpdateAuthorRequest request)
    {
        var author = await _authorService.UpdateAsync(id, request);
        if (author == null) return NotFound(new { message = $"Author with ID {id} not found." });
        return Ok(author);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await _authorService.DeleteAsync(id);
        if (!result) return NotFound(new { message = $"Author with ID {id} not found." });
        return NoContent();
    }
}
