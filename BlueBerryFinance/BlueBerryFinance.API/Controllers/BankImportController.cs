using BlueBerryFinance.API.Application.Features.BankImports;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BlueBerryFinance.API.Controllers
{
    [Route("bank-import")]
    [Authorize]
    public class BankImportController : BaseController
    {
        private readonly IBankImportHandler _handler;

        public BankImportController(IBankImportHandler handler)
        {
            _handler = handler;
        }

        [HttpPost("{bankAccountId:guid}")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> Import(Guid bankAccountId, IFormFile file)
        {
            if (file is null || file.Length == 0)
                return BadRequest(new { error = "No file provided." });

            if (!file.FileName.EndsWith(".csv", StringComparison.OrdinalIgnoreCase) &&
                !file.ContentType.Contains("text"))
                return BadRequest(new { error = "Only CSV files are accepted." });

            try
            {
                await using var stream = file.OpenReadStream();
                var result = await _handler.ImportAsync(bankAccountId, CurrentUserId, stream);
                return Ok(result);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                return HandleException(ex);
            }
        }
    }
}
