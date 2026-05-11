using BlueBerryFinance.API.Application.Features.AgentApproval;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BlueBerryFinance.API.Controllers
{
    [Authorize(Policy = "UserOrAdmin")]
    public class AgentApprovalController : BaseController
    {
        private readonly IAgentApprovalHandler _handler;
        private readonly ILogger<AgentApprovalController> _logger;

        public AgentApprovalController(IAgentApprovalHandler handler, ILogger<AgentApprovalController> logger)
        {
            _handler = handler;
            _logger = logger;
        }

        [HttpGet("agent-approvals/pending")]
        public async Task<IActionResult> GetPending(CancellationToken ct)
        {
            try
            {
                var items = await _handler.ListPendingAsync(CurrentUserId, ct);
                return Ok(items);
            }
            catch (Exception ex)
            {
                return HandleException(ex, _logger);
            }
        }

        [HttpPost("agent-approvals/{id:guid}/approve")]
        public async Task<IActionResult> Approve(Guid id, CancellationToken ct)
        {
            try
            {
                var result = await _handler.ApproveAsync(id, CurrentUserId, ct);

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
                return HandleException(ex, _logger);
            }
        }

        [HttpPost("agent-approvals/{id:guid}/reject")]
        public async Task<IActionResult> Reject(Guid id, CancellationToken ct)
        {
            try
            {
                var result = await _handler.RejectAsync(id, CurrentUserId, ct);

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
                return HandleException(ex, _logger);
            }
        }
    }
}
