using BlueBerryFinance.API.Application.Handlers.Interfaces;
using BlueBerryFinance.API.Application.Requests.Category;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BlueBerryFinance.API.Controllers
{
    [Route("api/v1.0/category")]
    [Authorize]
    public class CategoryController : BaseController
    {
        private readonly ICategoryHandler _handler;
        private readonly ILogger<CategoryController> _logger;

        public CategoryController(ICategoryHandler handler, ILogger<CategoryController> logger)
        {
            _handler = handler;
            _logger = logger;
        }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> List(CancellationToken ct)
        {
            try
            {
                var result = await _handler.ListAsync(ct);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return HandleException(ex, _logger);
            }
        }

        [HttpGet("{id:guid}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
        {
            try
            {
                var result = await _handler.GetByIdAsync(id, ct);
                return result is null ? NotFound() : Ok(result);
            }
            catch (Exception ex)
            {
                return HandleException(ex, _logger);
            }
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Register([FromBody] RegisterCategoryRequest request, CancellationToken ct)
        {
            try
            {
                var result = await _handler.RegisterAsync(request, ct);
                return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
            }
            catch (Exception ex)
            {
                return HandleException(ex, _logger);
            }
        }

        [HttpDelete("{id:guid}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [Authorize(Policy = "AdminOnly")]
        public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
        {
            try
            {
                var deleted = await _handler.DeleteAsync(id, ct);
                return deleted ? NoContent() : NotFound();
            }
            catch (Exception ex)
            {
                return HandleException(ex, _logger);
            }
        }
    }
}
