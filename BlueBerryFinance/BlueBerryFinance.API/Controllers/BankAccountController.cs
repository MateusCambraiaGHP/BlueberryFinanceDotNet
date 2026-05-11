using BlueBerryFinance.API.Application.Features.BankAccounts;
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
                var response = await _handler.GetAsync(filter, CurrentUserId);
                
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
        public async Task<IActionResult> Create([FromBody] RegisterBankAccountRequest request)
        {
            try
            {
                var response = await _handler.CreateAsync(request, CurrentUserId);

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
        public async Task<IActionResult> Delete(Guid id)
        {
            try
            {
                var response = await _handler.DeleteAsync(id, CurrentUserId);

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
