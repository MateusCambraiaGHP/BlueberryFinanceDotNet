using BlueBerryFinance.API.Data.Context;
using BlueBerryFinance.API.Data.Entities;
using BlueBerryFinance.API.Domain.Entities.Enums;
using BlueBerryFinance.API.Infrastructure.Services.Interfaces;
using BlueBerryFinance.API.Infrastructure.Utils.Agents.Finance.Interfaces;
using BlueBerryFinance.API.Infrastructure.Utils.Agents.Tools.Interfaces;
using Microsoft.Extensions.AI;
using System.ComponentModel;
using System.Security.Claims;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace BlueBerryFinance.API.Infrastructure.Utils.Agents.Tools
{
    /// <summary>
    /// Extract Processor Tool (PDF processing flow).
    /// Validates security, downloads the PDF, extracts transactions via FiscalNoteAgent,
    /// persists a FiscalNote record, and queues a bulk-import approval.
    /// </summary>
    public class ExtractProcessorTool : IExtractProcessorTool
    {
        private readonly AppDbContext _db;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IMinioService _minio;
        private readonly IFiscalNoteAgent _fiscalNoteAgent;
        private readonly ISecurityValidationAgent _validator;

        private static readonly JsonSerializerOptions _jsonOptions = new()
        {
            Converters = { new JsonStringEnumConverter() },
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        public ExtractProcessorTool(
            AppDbContext db,
            IHttpContextAccessor httpContextAccessor,
            IMinioService minio,
            IFiscalNoteAgent fiscalNoteAgent,
            ISecurityValidationAgent validator)
        {
            _db = db;
            _httpContextAccessor = httpContextAccessor;
            _minio = minio;
            _fiscalNoteAgent = fiscalNoteAgent;
            _validator = validator;
        }

        public AITool GetTool() => AIFunctionFactory.Create(
            ProcessAsync,
            "process_pdf_extract",
            "Downloads a PDF bank statement, extracts all transactions using AI, validates security, and queues them for user approval before bulk-importing.");

        private async Task<string> ProcessAsync(
            [Description("Bank account ID (UUID) where the transactions will be imported")] Guid bankAccountId,
            [Description("Public URL of the uploaded PDF bank statement")] string fileUrl)
        {
            var userId = GetCurrentUserId();

            // Step 1: Security validation
            var validationPrompt = $"Validate PDF import: userId={userId}, bankAccountId={bankAccountId}, fileUrl={fileUrl}";
            var validation = await _validator.AskAsync(validationPrompt);

            if (validation is null || !validation.IsValid)
                return $"Security validation failed: {validation?.Reason ?? "Unknown reason"}. PDF not processed.";

            // Step 2: Extract PDF text and classify transactions
            var objectName = ExtractObjectName(fileUrl);
            if (objectName is null)
                return $"Invalid file URL format: {fileUrl}";

            await using var pdfStream = await _minio.DownloadAsync(objectName);
            var transactions = await _fiscalNoteAgent.ExtractAsync(pdfStream);

            if (transactions.Count == 0)
                return "No transactions could be extracted from the PDF. Please verify it is a valid bank statement.";

            // Step 3: Persist FiscalNote (DB Save Tool)
            var fiscalNote = new FiscalNote
            {
                UserId = userId,
                ImageUrl = fileUrl,
                RawText = string.Empty,
                ExtractedData = JsonSerializer.Serialize(transactions, _jsonOptions),
                Status = FiscalNoteStatus.Pending
            };
            fiscalNote.SetInsertionDate(DateTime.UtcNow);
            fiscalNote.SetLastModification(DateTime.UtcNow);
            _db.FiscalNotes.Add(fiscalNote);
            await _db.SaveChangesAsync();

            // Step 4: Queue bulk-import approval (DB Save Tool)
            var items = transactions.Select(t => new ExtractedTransactionItem(
                t.Date.ToString("yyyy-MM-dd"),
                t.Description,
                t.Amount,
                t.Type)).ToList();

            var payload = new ProcessBankStatementPayload(bankAccountId, fiscalNote.Id, items);

            var approval = new AgentApproval
            {
                UserId = userId,
                AgentName = "BlueberryFinanceAgent",
                Tool = "process_pdf_extract",
                Payload = JsonSerializer.Serialize(payload, _jsonOptions),
                Status = ApprovalStatus.Pending,
                Active = 1
            };
            approval.SetInsertionDate(DateTime.UtcNow);
            approval.SetLastModification(DateTime.UtcNow);
            _db.AgentApprovals.Add(approval);
            await _db.SaveChangesAsync();

            return $"{transactions.Count} transaction(s) extracted, validated, and queued for bulk-import approval. " +
                   $"ApprovalId: {approval.Id}. Review and approve in the Approvals panel.";
        }

        private static string? ExtractObjectName(string url)
        {
            const string marker = "statements/";
            var idx = url.IndexOf(marker, StringComparison.Ordinal);
            return idx >= 0 ? url[idx..] : null;
        }

        private Guid GetCurrentUserId()
        {
            var claim = _httpContextAccessor.HttpContext?.User.FindFirstValue("userId");
            return Guid.TryParse(claim, out var id) ? id
                : throw new InvalidOperationException("Authenticated user not found in context.");
        }

        // ── Payload records ───────────────────────────────────────────────────────

        public record ExtractedTransactionItem(
            string Date,
            string Description,
            decimal Amount,
            string Type);

        public record ProcessBankStatementPayload(
            Guid BankAccountId,
            Guid FiscalNoteId,
            IReadOnlyList<ExtractedTransactionItem> Transactions);
    }
}
