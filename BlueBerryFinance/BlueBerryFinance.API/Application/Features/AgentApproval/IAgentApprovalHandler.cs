using BlueBerryFinance.Common.ViewModels;

namespace BlueBerryFinance.API.Application.Features.AgentApproval
{
    public interface IAgentApprovalHandler
    {
        Task<IReadOnlyList<AgentApprovalViewModel>> ListPendingAsync(Guid userId, CancellationToken ct = default);
        Task<AgentApprovalViewModel?> ApproveAsync(Guid approvalId, Guid userId, CancellationToken ct = default);
        Task<AgentApprovalViewModel?> RejectAsync(Guid approvalId, Guid userId, CancellationToken ct = default);
    }
}
