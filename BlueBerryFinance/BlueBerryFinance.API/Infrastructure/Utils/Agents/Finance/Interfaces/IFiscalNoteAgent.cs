namespace BlueBerryFinance.API.Infrastructure.Utils.Agents.Finance.Interfaces
{
    public interface IFiscalNoteAgent
    {
        /// <summary>
        /// Extracts transactions from a PDF bank statement stream.
        /// </summary>
        Task<IReadOnlyList<ExtractedTransaction>> ExtractAsync(
            Stream pdfStream,
            CancellationToken ct = default);
    }

    public record ExtractedTransaction(
        DateOnly Date,
        string Description,
        decimal Amount,
        string Type); // "Income" | "Expense"
}
