using BlueBerryFinance.API.Application.Handlers.Interfaces;
using BlueBerryFinance.API.Application.Requests;
using Microsoft.AspNetCore.Mvc;
using System.Transactions;

namespace BlueBerryFinance.API.Controllers
{
    [Route("transaction/")]
    public class TransactionController : Controller
    {
        private readonly ITransactionsHandler _handler;

        public TransactionController(ITransactionsHandler handler)
        {
            _handler = handler;
        }

        [HttpGet]
        public ActionResult<List<Transaction>> Get()
        {
            return new List<Transaction>();
        }

        [HttpPost("analyze")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Analyze(
        [FromBody] FinancialAnalysisRequest request,
        CancellationToken cancellationToken)
        {
            var result = await _handler.HandleAsync(request, cancellationToken);

            if (result is null)
                return BadRequest("Failed to process the request");

            return Ok(result);
        }
    }
}
