using BlueBerryFinance.Common.ViewModels;

namespace BlueBerryFinance.API.Application.Features.AgentApproval
{
    public interface IAgentApprovalHandler
    {
        Task<IReadOnlyList<AgentApprovalViewModel>> ListPendingAsync(Guid userId);
        Task<AgentApprovalViewModel?> ApproveAsync(Guid approvalId, Guid userId);
        Task<AgentApprovalViewModel?> RejectAsync(Guid approvalId, Guid userId);
    }
}
