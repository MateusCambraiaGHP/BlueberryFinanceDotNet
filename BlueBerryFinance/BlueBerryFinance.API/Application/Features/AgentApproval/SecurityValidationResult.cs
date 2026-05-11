using System.ComponentModel;

namespace BlueBerryFinance.API.Application.Features.AgentApproval
{
    public class SecurityValidationResult
    {
        [Description("Whether the proposed operation is safe and valid for this user")]
        public bool IsValid { get; set; }

        [Description("Reason for the validation decision")]
        public string Reason { get; set; } = string.Empty;
    }
}
