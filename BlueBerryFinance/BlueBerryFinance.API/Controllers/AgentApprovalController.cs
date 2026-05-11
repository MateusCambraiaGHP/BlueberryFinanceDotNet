using BlueBerryFinance.API.Application.Features.AgentApprovals;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace BlueBerryFinance.API.Controllers
{
    [Authorize(Policy = "UserOrAdmin")]
    [Route("agent-approvals")]
    public class AgentApprovalController : BaseController
    {
        private readonly IAgentApprovalHandler _handler;

        public AgentApprovalController(IAgentApprovalHandler handler)
            => _handler = handler;

        [HttpGet("pending")]
        public async Task<IActionResult> Get([FromQuery] AgentApprovalFilterRequest filter)
        {
            try
            {
                var response = await _handler.GetAsync(CurrentUserId, filter);

                if (!response.Success)
                    return BadRequest(response);

                return Ok(response);
            }
            catch (Exception ex)
            {
                return HandleException(ex);
            }
        }

        [HttpPost("{id:guid}/approve")]
        public async Task<IActionResult> Approve(Guid id)
        {
            try
            {
                var response = await _handler.ApproveAsync(id, CurrentUserId);

                if (!response.Success)
                    return BadRequest(response);

                return Ok(response);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return HandleException(ex);
            }
        }

        [HttpPost("{id:guid}/reject")]
        public async Task<IActionResult> Reject(Guid id)
        {
            try
            {
                var response = await _handler.RejectAsync(id, CurrentUserId);

                if (!response.Success)
                    return BadRequest(response);

                return Ok(response);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return HandleException(ex);
            }
        }
    }
}
