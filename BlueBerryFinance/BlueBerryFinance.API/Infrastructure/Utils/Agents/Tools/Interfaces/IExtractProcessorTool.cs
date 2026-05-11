using Microsoft.Extensions.AI;

namespace BlueBerryFinance.API.Infrastructure.Utils.Agents.Tools.Interfaces
{
    /// <summary>
    /// Tool that processes PDF bank statements, extracts transactions, and queues them for bulk import.
    /// Exposes: process_pdf_extract.
    /// </summary>
    public interface IExtractProcessorTool
    {
        AITool GetTool();
    }
}
