using BlueBerryFinance.API.Application.Features.Common;
using BlueBerryFinance.Common.ViewModels;

namespace BlueBerryFinance.API.Application.Features.AgentApprovals
{
    public interface IAgentApprovalHandler
    {
        Task<BaseResponse<AgentApprovalViewModel>> GetAsync(Guid userId, AgentApprovalFilterRequest filter);
        Task<BaseResponse<AgentApprovalViewModel>> ApproveAsync(Guid approvalId, Guid userId);
        Task<BaseResponse<AgentApprovalViewModel>> RejectAsync(Guid approvalId, Guid userId);
    }
}
