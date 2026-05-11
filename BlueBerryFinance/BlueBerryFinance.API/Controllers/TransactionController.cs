using BlueBerryFinance.API.Application.Features.Transaction;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BlueBerryFinance.API.Controllers
{
    [Route("transaction")]
    [Authorize]
    public class TransactionController : BaseController
    {
        private readonly ITransactionHandler _handler;

        public TransactionController(ITransactionHandler handler)
        {
            _handler = handler;
        }

        [HttpGet]
        public async Task<IActionResult> Get([FromQuery] ListTransactionsRequest request)
        {
            try
            {
                var result = await _handler.GetAsync(request, CurrentUserId);

                if (request.Id.HasValue && result.TotalCount == 0)
                    return NotFound();

                return Ok(result);
            }
            catch (Exception ex)
            {
                return HandleException(ex);
            }
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] RegisterTransactionRequest request)
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

        [HttpPut("{id:guid}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateTransactionRequest request)
        {
            try
            {
                request.Id = id;
                var result = await _handler.UpdateAsync(request, CurrentUserId);
                return result is null ? NotFound() : Ok(result);
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
