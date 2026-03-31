using BlueBerryFinance.API.Application.Handlers.Interfaces;
using BlueBerryFinance.API.Controllers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BlueBerryFinance.API.Controllers
{
    [Authorize(Policy = "UserOrAdmin")]
    public class ReportController : BaseController
    {
        private readonly IReportHandler _handler;
        private readonly ILogger<ReportController> _logger;

        public ReportController(IReportHandler handler, ILogger<ReportController> logger)
        {
            _handler = handler;
            _logger  = logger;
        }

        /// <summary>GET /api/v1.0/report/monthly?year=2025&month=3</summary>
        [HttpGet("report/monthly")]
        public async Task<IActionResult> GetMonthly(
            [FromQuery] int year, [FromQuery] int month, CancellationToken ct)
        {
            if (year < 2000 || year > 2100 || month < 1 || month > 12)
                return BadRequest(new { message = "Invalid year or month." });

            try
            {
                var result = await _handler.GetMonthlyAsync(year, month, CurrentUserId, ct);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return HandleException(ex, _logger);
            }
        }
    }
}
