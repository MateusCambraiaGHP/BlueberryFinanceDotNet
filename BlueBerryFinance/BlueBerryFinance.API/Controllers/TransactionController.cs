using BlueBerryFinance.API.Application.Features.Transactions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BlueBerryFinance.API.Controllers
{
    [Route("transaction")]
    [Authorize]
    public class TransactionController : BaseController
    {
        private readonly ITransactionHandler _handler;

        public TransactionController(ITransactionHandler handler)
        {
            _handler = handler;
        }

        [HttpGet]
        public async Task<IActionResult> Get([FromQuery] ListTransactionsRequest request)
        {
            try
            {
                var response = await _handler.GetAsync(request, CurrentUserId);

                if (!response.Success)
                    return BadRequest(response);

                return Ok(response);
            }
            catch (Exception ex)
            {
                return HandleException(ex);
            }
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] RegisterTransactionRequest request)
        {
            try
            {
                var response = await _handler.CreateAsync(request, CurrentUserId);

                if (!response.Success)
                    return BadRequest(response);

                return Ok(response);
            }
            catch (Exception ex)
            {
                return HandleException(ex);
            }
        }

        [HttpPut("{id:guid}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateTransactionRequest request)
        {
            try
            {
                request.Id = id;
                var response = await _handler.UpdateAsync(request, CurrentUserId);


                if (!response.Success)
                    return BadRequest(response);

                return Ok(response);
            }
            catch (Exception ex)
            {
                return HandleException(ex);
            }
        }

        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            try
            {
                var response = await _handler.DeleteAsync(id, CurrentUserId);

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
