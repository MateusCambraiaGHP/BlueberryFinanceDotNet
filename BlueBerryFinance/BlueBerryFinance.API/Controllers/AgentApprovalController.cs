using BlueBerryFinance.API.Application.Features.AgentApproval;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BlueBerryFinance.API.Controllers
{
    [Authorize(Policy = "UserOrAdmin")]
    [Route("agent-approvals")]
    public class AgentApprovalController : BaseController
    {
        private readonly IAgentApprovalHandler _handler;

        public AgentApprovalController(IAgentApprovalHandler handler)
        {
            _handler = handler;
        }

        [HttpGet("pending")]
        public async Task<IActionResult> GetPending()
        {
            try
            {
                var items = await _handler.ListPendingAsync(CurrentUserId);
                return Ok(items);
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
                var result = await _handler.ApproveAsync(id, CurrentUserId);

                if (result is null)
                    return NotFound(new { message = "Approval not found." });

                return Ok(result);
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
                var result = await _handler.RejectAsync(id, CurrentUserId);

                if (result is null)
                    return NotFound(new { message = "Approval not found." });

                return Ok(result);
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
