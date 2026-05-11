using BlueBerryFinance.API.Domain.Entities.Enums;

namespace BlueBerryFinance.API.Application.Features.AgentApprovals
{
    public class AgentApprovalFilterRequest
    {
        public ApprovalStatus? Status { get; set; }
    }
}
