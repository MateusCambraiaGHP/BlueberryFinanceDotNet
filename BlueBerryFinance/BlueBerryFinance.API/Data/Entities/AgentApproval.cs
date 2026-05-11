using BlueBerryFinance.API.Data.Entities.Enums;
using BlueBerryFinance.Common.DomainObjects;

namespace BlueBerryFinance.API.Data.Entities
{
    public class AgentApproval : EntityBase
    {
        public Guid UserId { get; set; }
        public string AgentName { get; set; } = string.Empty;
        public string Tool { get; set; } = string.Empty;
        public string Payload { get; set; } = "{}"; // JSON
        public ApprovalStatus Status { get; set; } = ApprovalStatus.Pending;
        public DateTime? ResolvedAt { get; set; }

        public User User { get; set; } = null!;

        public AgentApproval() { }
    }
}
