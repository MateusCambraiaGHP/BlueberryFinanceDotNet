using BlueBerryFinance.API.Application.Handlers.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BlueBerryFinance.API.Controllers
{
    [Route("api/v1.0/csv-import")]
    [Authorize]
    public class CsvImportController : BaseController
    {
        private readonly ICsvImportHandler _handler;
        private readonly ILogger<CsvImportController> _logger;

        public CsvImportController(ICsvImportHandler handler, ILogger<CsvImportController> logger)
        {
            _handler = handler;
            _logger = logger;
        }

        /// <summary>
        /// Upload a CSV bank statement export and import transactions into the given bank account.
        /// Supported formats: ActivoBank, CGD, BPI, Millennium BCP (semicolon-separated Portuguese CSV).
        /// Duplicate rows (same date + amount + description) are automatically skipped.
        /// </summary>
        [HttpPost("{bankAccountId:guid}")]
        [Consumes("multipart/form-data")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Import(Guid bankAccountId, IFormFile file, CancellationToken ct)
        {
            if (file is null || file.Length == 0)
                return BadRequest(new { error = "No file provided." });

            if (!file.FileName.EndsWith(".csv", StringComparison.OrdinalIgnoreCase) &&
                !file.ContentType.Contains("text"))
                return BadRequest(new { error = "Only CSV files are accepted." });

            try
            {
                await using var stream = file.OpenReadStream();
                var result = await _handler.ImportAsync(bankAccountId, CurrentUserId, stream, ct);
                return Ok(result);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                return HandleException(ex, _logger);
            }
        }
    }
}
