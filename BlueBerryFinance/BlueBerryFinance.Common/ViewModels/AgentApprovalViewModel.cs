namespace BlueBerryFinance.Common.ViewModels
{
    public class AgentApprovalViewModel
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public string AgentName { get; set; } = string.Empty;
        public string Tool { get; set; } = string.Empty;
        public string Payload { get; set; } = "{}";
        public string Status { get; set; } = string.Empty;
        public DateTime? ResolvedAt { get; set; }
        public DateTime InsertionDate { get; set; }

        public AgentApprovalViewModel(
            Guid id,
            Guid userId,
            string agentName,
            string tool,
            string payload,
            string status,
            DateTime? resolvedAt,
            DateTime insertionDate)
        {
            Id = id;
            UserId = userId;
            AgentName = agentName;
            Tool = tool;
            Payload = payload;
            Status = status;
            ResolvedAt = resolvedAt;
            InsertionDate = insertionDate;
        }
    }
}
