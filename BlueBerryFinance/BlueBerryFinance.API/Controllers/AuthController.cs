using BlueBerryFinance.API.Application.Handlers.Interfaces;
using BlueBerryFinance.API.Application.Requests.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BlueBerryFinance.API.Controllers
{
    [Route("api/v1.0/auth")]
    public class AuthController : BaseController
    {
        private readonly IAuthHandler _handler;
        private readonly ILogger<AuthController> _logger;

        public AuthController(IAuthHandler handler, ILogger<AuthController> logger)
        {
            _handler = handler;
            _logger = logger;
        }

        [HttpPost("login")]
        [AllowAnonymous]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> Login([FromBody] LoginRequest request, CancellationToken ct)
        {
            try
            {
                var result = await _handler.HandleAsync(request, ct);

                if (result is null)
                    return Unauthorized(new { message = "Invalid credentials." });

                return Ok(result);
            }
            catch (Exception ex)
            {
                return HandleException(ex, _logger);
            }
        }
    }
}
