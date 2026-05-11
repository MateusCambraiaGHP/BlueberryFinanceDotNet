using BlueBerryFinance.API.Application.Features.Currency;
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
                var result = await _handler.GetAsync();
                return Ok(result);
            }
            catch (Exception ex)
            {
                return HandleException(ex);
            }
        }
    }
}
