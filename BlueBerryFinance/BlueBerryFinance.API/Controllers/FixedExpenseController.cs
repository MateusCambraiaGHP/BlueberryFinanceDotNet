using BlueBerryFinance.API.Application.Features.FixedExpense;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BlueBerryFinance.API.Controllers
{
    [Route("fixed-expense")]
    [Authorize]
    public class FixedExpenseController : BaseController
    {
        private readonly IFixedExpenseHandler _handler;

        public FixedExpenseController(IFixedExpenseHandler handler)
        {
            _handler = handler;
        }

        [HttpGet]
        public async Task<IActionResult> Get([FromQuery] FixedExpenseFilterRequest filter)
        {
            try
            {
                var result = await _handler.GetAsync(filter, CurrentUserId);

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
        public async Task<IActionResult> Create([FromBody] RegisterFixedExpenseRequest request)
        {
            try
            {
                var result = await _handler.RegisterAsync(request, CurrentUserId);
                return CreatedAtAction(nameof(Get), new { id = result.Id }, result);
            }
            catch (Exception ex)
            {
                return HandleException(ex);
            }
        }

        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            try
            {
                var deleted = await _handler.DeleteAsync(id, CurrentUserId);
                return deleted ? NoContent() : NotFound();
            }
            catch (Exception ex)
            {
                return HandleException(ex);
            }
        }
    }
}
