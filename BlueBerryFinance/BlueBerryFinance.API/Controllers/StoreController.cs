using BlueBerryFinance.API.Application.Features.Stores;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BlueBerryFinance.API.Controllers
{
    [Route("store")]
    [Authorize]
    public class StoreController : BaseController
    {
        private readonly IStoreHandler _handler;

        public StoreController(IStoreHandler handler)
        {
            _handler = handler;
        }

        [HttpGet]
        public async Task<IActionResult> Get([FromQuery] StoreFilterRequest filter)
        {
            try
            {
                var response = await _handler.GetAsync(filter);

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
        public async Task<IActionResult> Create([FromBody] RegisterStoreRequest request)
        {
            try
            {
                var response = await _handler.CreateAsync(request);

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
                var response = await _handler.DeleteAsync(id);
                
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
