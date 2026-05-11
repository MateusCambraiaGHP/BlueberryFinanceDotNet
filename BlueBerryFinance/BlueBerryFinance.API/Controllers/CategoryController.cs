using BlueBerryFinance.API.Application.Features.Category;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BlueBerryFinance.API.Controllers
{
    [Route("category")]
    [Authorize]
    public class CategoryController : BaseController
    {
        private readonly ICategoryHandler _handler;

        public CategoryController(ICategoryHandler handler)
        {
            _handler = handler;
        }

        [HttpGet]
        public async Task<IActionResult> Get([FromQuery] CategoryFilterRequest filter)
        {
            try
            {
                var result = await _handler.GetAsync(filter);

                if (filter.Id.HasValue && result.Count == 0)
                    return NotFound();

                return Ok(result);
            }
            catch (Exception ex)
            {
                return HandleException(ex);
            }
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] RegisterCategoryRequest request)
        {
            try
            {
                var result = await _handler.RegisterAsync(request);
                return CreatedAtAction(nameof(Get), new { id = result.Id }, result);
            }
            catch (Exception ex)
            {
                return HandleException(ex);
            }
        }

        [HttpDelete("{id:guid}")]
        [Authorize(Policy = "AdminOnly")]
        public async Task<IActionResult> Delete(Guid id)
        {
            try
            {
                var deleted = await _handler.DeleteAsync(id);
                return deleted ? NoContent() : NotFound();
            }
            catch (Exception ex)
            {
                return HandleException(ex);
            }
        }
    }
}
