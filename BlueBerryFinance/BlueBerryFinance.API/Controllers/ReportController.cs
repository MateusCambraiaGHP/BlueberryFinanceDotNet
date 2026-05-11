using BlueBerryFinance.API.Application.Features.Reports;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BlueBerryFinance.API.Controllers
{
    [Route("report")]
    [Authorize(Policy = "UserOrAdmin")]
    public class ReportController : BaseController
    {
        private readonly IReportHandler _handler;

        public ReportController(IReportHandler handler)
        {
            _handler = handler;
        }

        [HttpGet("monthly")]
        public async Task<IActionResult> GetMonthly(
            [FromQuery] GetMonthlyReportRequest request)
        {
            try
            {
                var response = await _handler.GetMonthlyAsync(
                    request.Year,
                    request.Month,
                    CurrentUserId);

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
