using BlueBerryFinance.API.Application.Features.BankAccount;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BlueBerryFinance.API.Controllers
{
    [Route("bank-account")]
    [Authorize]
    public class BankAccountController : BaseController
    {
        private readonly IBankAccountHandler _handler;

        public BankAccountController(IBankAccountHandler handler)
        {
            _handler = handler;
        }

        [HttpGet]
        public async Task<IActionResult> Get([FromQuery] BankAccountFilterRequest filter)
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
        public async Task<IActionResult> Create([FromBody] RegisterBankAccountRequest request)
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
