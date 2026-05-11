using BlueBerryFinance.API.Application.Features.Transaction;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BlueBerryFinance.API.Controllers
{
    [Route("api/v1.0/transaction")]
    [Authorize]
    public class TransactionController : BaseController
    {
        private readonly ITransactionHandler _handler;
        private readonly ILogger<TransactionController> _logger;

        public TransactionController(ITransactionHandler handler, ILogger<TransactionController> logger)
        {
            _handler = handler;
            _logger = logger;
        }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Get([FromQuery] ListTransactionsRequest request, CancellationToken ct)
        {
            try
            {
                var result = await _handler.GetAsync(request, CurrentUserId, ct);

                if (request.Id.HasValue && result.TotalCount == 0)
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
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Register([FromBody] RegisterTransactionRequest request, CancellationToken ct)
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

        [HttpPut("{id:guid}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateTransactionRequest request, CancellationToken ct)
        {
            try
            {
                request.Id = id;
                var result = await _handler.UpdateAsync(request, CurrentUserId, ct);
                return result is null ? NotFound() : Ok(result);
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
