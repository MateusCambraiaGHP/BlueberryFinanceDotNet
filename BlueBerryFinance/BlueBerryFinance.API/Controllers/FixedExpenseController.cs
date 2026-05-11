using BlueBerryFinance.API.Application.Features.FixedExpenses;
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
        public async Task<IActionResult> Create([FromBody] RegisterFixedExpenseRequest request)
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
