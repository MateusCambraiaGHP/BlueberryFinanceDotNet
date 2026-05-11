using BlueBerryFinance.API.Application.Features.FixedExpense;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BlueBerryFinance.API.Controllers
{
    [Route("api/v1.0/fixed-expense")]
    [Authorize]
    public class FixedExpenseController : BaseController
    {
        private readonly IFixedExpenseHandler _handler;
        private readonly ILogger<FixedExpenseController> _logger;

        public FixedExpenseController(IFixedExpenseHandler handler, ILogger<FixedExpenseController> logger)
        {
            _handler = handler;
            _logger = logger;
        }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Get([FromQuery] FixedExpenseFilterRequest filter, CancellationToken ct)
        {
            try
            {
                var result = await _handler.GetAsync(filter, CurrentUserId, ct);

                if (filter.Id.HasValue && result.Count == 0)
                    return NotFound();

                return Ok(result);
            }
            catch (Exception ex)
            {
                return HandleException(ex, _logger);
            }
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        public async Task<IActionResult> Register([FromBody] RegisterFixedExpenseRequest request, CancellationToken ct)
        {
            try
            {
                var result = await _handler.RegisterAsync(request, CurrentUserId, ct);
                return CreatedAtAction(nameof(Get), new { id = result.Id }, result);
            }
            catch (Exception ex)
            {
                return HandleException(ex, _logger);
            }
        }

        [HttpDelete("{id:guid}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
        {
            try
            {
                var deleted = await _handler.DeleteAsync(id, CurrentUserId, ct);
                return deleted ? NoContent() : NotFound();
            }
            catch (Exception ex)
            {
                return HandleException(ex, _logger);
            }
        }
    }
}
