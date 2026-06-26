using Microsoft.AspNetCore.Mvc;
using DBOperationWithEFCore.DTOs;
using DBOperationWithEFCore.Services.Interfaces;

namespace DBOperationWithEFCore.Controllers;

/// <summary>
/// Books API Controller.
/// Demonstrates: [ApiController], model validation, ActionResult patterns, query parameters.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class BooksController : ControllerBase
{
    private readonly IBookService _bookService;

    public BooksController(IBookService bookService)
    {
        _bookService = bookService;
    }

    /// <summary>
    /// Get paginated list of books with optional search and filters.
    /// GET /api/books?page=1&pageSize=10&search=harry&categoryId=1&authorId=2
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<PagedResult<BookDto>>> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? search = null,
        [FromQuery] int? categoryId = null,
        [FromQuery] int? authorId = null)
    {
        if (page < 1) page = 1;
        if (pageSize < 1 || pageSize > 50) pageSize = 10;

        var result = await _bookService.GetAllAsync(page, pageSize, search, categoryId, authorId);
        return Ok(result);
    }

    /// <summary>
    /// Get a single book by ID.
    /// GET /api/books/5
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<BookDto>> GetById(int id)
    {
        var book = await _bookService.GetByIdAsync(id);
        if (book == null) return NotFound(new { message = $"Book with ID {id} not found." });
        return Ok(book);
    }

    /// <summary>
    /// Create a new book.
    /// POST /api/books
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<BookDto>> Create([FromBody] CreateBookRequest request)
    {
        var book = await _bookService.CreateAsync(request);
        return CreatedAtAction(nameof(GetById), new { id = book.Id }, book);
    }

    /// <summary>
    /// Update an existing book.
    /// PUT /api/books/5
    /// </summary>
    [HttpPut("{id}")]
    public async Task<ActionResult<BookDto>> Update(int id, [FromBody] UpdateBookRequest request)
    {
        var book = await _bookService.UpdateAsync(id, request);
        if (book == null) return NotFound(new { message = $"Book with ID {id} not found." });
        return Ok(book);
    }

    /// <summary>
    /// Soft-delete a book.
    /// DELETE /api/books/5
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await _bookService.DeleteAsync(id);
        if (!result) return NotFound(new { message = $"Book with ID {id} not found." });
        return NoContent();
    }
}
