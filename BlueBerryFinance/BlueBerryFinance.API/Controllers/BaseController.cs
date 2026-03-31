using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BlueBerryFinance.API.Controllers
{
    [ApiController]
    [Route("api/v1.0/")]
    public abstract class BaseController : ControllerBase
    {
        protected BaseController() { }

        protected Guid CurrentUserId
        {
            get
            {
                var claim = User.FindFirstValue("userId");
                return Guid.TryParse(claim, out var id) ? id : Guid.Empty;
            }
        }

        protected IActionResult HandleException(Exception ex, ILogger logger)
        {
            logger.LogError(ex, "Unhandled exception");
            return StatusCode(500, new { message = "An unexpected error occurred." });
        }
    }
}
