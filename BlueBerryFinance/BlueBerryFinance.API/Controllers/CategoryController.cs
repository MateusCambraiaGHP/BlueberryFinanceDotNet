using BlueBerryFinance.API.Application.Features.Categories;
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
                var response = await _handler.GetAsync(filter);

                if (!response.Success)
                    return BadRequest(response);

                return Ok(response);
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
                var response = await _handler.CreateAsync(request);

                if (!response.Success)
                    return BadRequest(response);

                return Ok(response);
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
                var response = await _handler.DeleteAsync(id);

                if (!response.Success)
                    return BadRequest(response);

                return Ok(response);
            }
            catch (Exception ex)
            {
                return HandleException(ex);
            }
        }
    }
}
