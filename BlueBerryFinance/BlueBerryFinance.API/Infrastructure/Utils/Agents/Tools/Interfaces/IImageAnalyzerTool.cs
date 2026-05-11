using Microsoft.Extensions.AI;

namespace BlueBerryFinance.API.Infrastructure.Utils.Agents.Tools.Interfaces
{
    /// <summary>
    /// Tool that analyzes receipt/invoice images using vision AI and queues a transaction approval.
    /// Exposes: analyze_image.
    /// </summary>
    public interface IImageAnalyzerTool
    {
        AITool GetTool();
    }
}
