using BlueBerryFinance.API.Domain.Entities.Enums;
using BlueBerryFinance.API.Domain.Exceptions;
using BlueBerryFinance.Common.DomainObjects;

namespace BlueBerryFinance.API.Domain.Entities
{
    public class AgentApproval : EntityBase
    {
        public Guid UserId { get; set; }
        public string AgentName { get; set; } = string.Empty;
        public string Tool { get; set; } = string.Empty;
        public string Payload { get; set; } = "{}";
        public ApprovalStatus Status { get; set; } = ApprovalStatus.Pending;
        public DateTime? ResolvedAt { get; set; }
        public User User { get; set; } = null!;

        public AgentApproval() { }

        public void Approve()
        {
            if (Status != ApprovalStatus.Pending)
                throw new BusinessRuleException($"Approval is already {Status}.");
        }

        public void Reject()
        {
            if (Status != ApprovalStatus.Pending)
                throw new BusinessRuleException($"Approval is already {Status}.");
        }
    }
}
