using BlueBerryFinance.API.Application.Features.Report;
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

        [HttpGet("report/monthly")]
        public async Task<IActionResult> GetMonthly(
            [FromQuery] GetMonthlyReportRequest request, CancellationToken ct)
        {
            try
            {
                var result = await _handler.GetMonthlyAsync(request.Year, request.Month, CurrentUserId, ct);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return HandleException(ex, _logger);
            }
        }
    }
}
