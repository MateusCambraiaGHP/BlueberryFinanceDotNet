using BlueBerryFinance.API.Application.Handlers.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BlueBerryFinance.API.Controllers
{
    [Route("api/v1.0/currency")]
    [Authorize]
    public class CurrencyController : BaseController
    {
        private readonly ICurrencyHandler _handler;
        private readonly ILogger<CurrencyController> _logger;

        public CurrencyController(ICurrencyHandler handler, ILogger<CurrencyController> logger)
        {
            _handler = handler;
            _logger = logger;
        }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> List(CancellationToken ct)
        {
            try
            {
                var result = await _handler.ListAsync(ct);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return HandleException(ex, _logger);
            }
        }
    }
}
