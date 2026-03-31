using BlueBerryFinance.API.Application.Handlers.Interfaces;
using BlueBerryFinance.API.Application.Requests.Transaction;
using BlueBerryFinance.API.Data.Context;
using BlueBerryFinance.API.Data.Entities.Enums;
using BlueBerryFinance.Common.ViewModels;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using System.Text.Json.Serialization;
using static BlueBerryFinance.API.Infrastructure.Utils.Agents.Tools.AgentWriteTools;
using static BlueBerryFinance.API.Infrastructure.Utils.Agents.Tools.ExtractProcessorTool;

namespace BlueBerryFinance.API.Application.Handlers
{
    public class AgentApprovalHandler : IAgentApprovalHandler
    {
        private readonly AppDbContext _db;
        private readonly ITransactionHandler _transactionHandler;

        private static readonly JsonSerializerOptions _jsonOptions = new()
        {
            Converters = { new JsonStringEnumConverter() },
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        public AgentApprovalHandler(AppDbContext db, ITransactionHandler transactionHandler)
        {
            _db = db;
            _transactionHandler = transactionHandler;
        }

        public async Task<IReadOnlyList<AgentApprovalViewModel>> ListPendingAsync(
            Guid userId, CancellationToken ct = default)
        {
            return await _db.AgentApprovals
                .AsNoTracking()
                .Where(a => a.UserId == userId && a.Status == ApprovalStatus.Pending)
                .OrderByDescending(a => a.InsertionDate)
                .Select(a => new AgentApprovalViewModel
                {
                    Id = a.Id,
                    UserId = a.UserId,
                    AgentName = a.AgentName,
                    Tool = a.Tool,
                    Payload = a.Payload,
                    Status = a.Status.ToString(),
                    ResolvedAt = a.ResolvedAt,
                    InsertionDate = a.InsertionDate
                })
                .ToListAsync(ct);
        }

        public async Task<AgentApprovalViewModel?> ApproveAsync(
            Guid approvalId, Guid userId, CancellationToken ct = default)
        {
            var approval = await _db.AgentApprovals
                .FirstOrDefaultAsync(a => a.Id == approvalId && a.UserId == userId, ct);

            if (approval is null) return null;

            if (approval.Status != ApprovalStatus.Pending)
                throw new InvalidOperationException($"Approval is already {approval.Status}.");

            await ExecuteToolAsync(approval, userId, ct);

            approval.Status = ApprovalStatus.Approved;
            approval.ResolvedAt = DateTime.UtcNow;
            approval.SetLastModification(DateTime.UtcNow);

            await _db.SaveChangesAsync(ct);

            return ToViewModel(approval);
        }

        public async Task<AgentApprovalViewModel?> RejectAsync(
            Guid approvalId, Guid userId, CancellationToken ct = default)
        {
            var approval = await _db.AgentApprovals
                .FirstOrDefaultAsync(a => a.Id == approvalId && a.UserId == userId, ct);

            if (approval is null) return null;

            if (approval.Status != ApprovalStatus.Pending)
                throw new InvalidOperationException($"Approval is already {approval.Status}.");

            approval.Status = ApprovalStatus.Rejected;
            approval.ResolvedAt = DateTime.UtcNow;
            approval.SetLastModification(DateTime.UtcNow);

            await _db.SaveChangesAsync(ct);

            return ToViewModel(approval);
        }

        // ── Tool dispatch ─────────────────────────────────────────────────────────

        private async Task ExecuteToolAsync(
            Data.Entities.AgentApproval approval, Guid userId, CancellationToken ct)
        {
            switch (approval.Tool)
            {
                // ── Orchestrator tools (BlueberryFinanceAgent) ────────────────────
                case "save_income_expense":
                case "analyze_image":
                case "create_transaction":          // legacy — same payload
                {
                    var payload = JsonSerializer.Deserialize<CreateTransactionPayload>(approval.Payload, _jsonOptions)
                        ?? throw new InvalidOperationException($"Invalid {approval.Tool} payload.");

                    var req = new RegisterTransactionRequest
                    {
                        BankAccountId   = payload.BankAccountId,
                        StoreId         = payload.StoreId,
                        CategoryId      = payload.CategoryId,
                        CurrencyId      = payload.CurrencyId,
                        FixedExpenseId  = payload.FixedExpenseId,
                        OriginType      = Enum.Parse<OriginType>(payload.OriginType, ignoreCase: true),
                        TransactionType = Enum.Parse<TransactionType>(payload.TransactionType, ignoreCase: true),
                        Source          = TransactionSource.Agent,
                        Amount          = payload.Amount,
                        Description     = payload.Description,
                        TransactionDate = payload.TransactionDate.Kind == DateTimeKind.Utc
                            ? payload.TransactionDate
                            : DateTime.SpecifyKind(payload.TransactionDate, DateTimeKind.Utc)
                    };

                    await _transactionHandler.RegisterAsync(req, userId, ct);
                    break;
                }

                case "delete_income_expense":
                case "delete_transaction":          // legacy — same payload
                {
                    var payload = JsonSerializer.Deserialize<DeleteTransactionPayload>(approval.Payload, _jsonOptions)
                        ?? throw new InvalidOperationException($"Invalid {approval.Tool} payload.");

                    await _transactionHandler.DeleteAsync(payload.TransactionId, userId, ct);
                    break;
                }

                case "process_pdf_extract":
                case "process_bank_statement":      // legacy
                {
                    // Bulk-import: individual transactions require separate approval per row.
                    // Future: iterate payload.Transactions and register each one.
                    var payload = JsonSerializer.Deserialize<ProcessBankStatementPayload>(approval.Payload, _jsonOptions)
                        ?? throw new InvalidOperationException($"Invalid {approval.Tool} payload.");

                    foreach (var item in payload.Transactions)
                    {
                        if (!DateTime.TryParse(item.Date, out var date)) continue;
                        date = DateTime.SpecifyKind(date, DateTimeKind.Utc);
                        if (!Enum.TryParse<TransactionType>(item.Type, ignoreCase: true, out var txType)) continue;

                        var req = new RegisterTransactionRequest
                        {
                            BankAccountId   = payload.BankAccountId,
                            StoreId         = Guid.Empty,
                            CategoryId      = Guid.Empty,
                            CurrencyId      = Guid.Empty,
                            FixedExpenseId  = null,
                            OriginType      = OriginType.Company,
                            TransactionType = txType,
                            Source          = TransactionSource.Agent,
                            Amount          = item.Amount,
                            Description     = item.Description,
                            TransactionDate = date
                        };

                        await _transactionHandler.RegisterAsync(req, userId, ct);
                    }
                    break;
                }

                default:
                    throw new InvalidOperationException($"Unknown agent tool: '{approval.Tool}'.");
            }
        }

        private static AgentApprovalViewModel ToViewModel(Data.Entities.AgentApproval a) => new()
        {
            Id            = a.Id,
            UserId        = a.UserId,
            AgentName     = a.AgentName,
            Tool          = a.Tool,
            Payload       = a.Payload,
            Status        = a.Status.ToString(),
            ResolvedAt    = a.ResolvedAt,
            InsertionDate = a.InsertionDate
        };
    }
}
