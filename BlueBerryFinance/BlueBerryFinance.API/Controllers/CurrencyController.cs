using BlueBerryFinance.API.Application.Features.Currencies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BlueBerryFinance.API.Controllers
{
    [Route("currency")]
    [Authorize]
    public class CurrencyController : BaseController
    {
        private readonly ICurrencyHandler _handler;

        public CurrencyController(ICurrencyHandler handler)
        {
            _handler = handler;
        }

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            try
            {
                var response = await _handler.GetAsync();
                
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
