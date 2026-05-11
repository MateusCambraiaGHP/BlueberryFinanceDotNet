using BlueBerryFinance.API.Application.Features.Common;
using BlueBerryFinance.API.Application.Features.Transaction;
using BlueBerryFinance.API.Application.Features.Transactions;
using BlueBerryFinance.API.Data.Context;
using BlueBerryFinance.API.Domain.Entities;
using BlueBerryFinance.API.Domain.Entities.Enums;
using BlueBerryFinance.API.Infrastructure.Utils;
using BlueBerryFinance.Common.ViewModels;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using System.Text.Json.Serialization;
using static BlueBerryFinance.API.Infrastructure.Utils.Agents.Tools.AgentWriteTools;
using static BlueBerryFinance.API.Infrastructure.Utils.Agents.Tools.ExtractProcessorTool;

namespace BlueBerryFinance.API.Application.Features.AgentApprovals
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

        public async Task<BaseResponse<AgentApprovalViewModel>> GetAsync(Guid userId, AgentApprovalFilterRequest filter)
        {
            var statusFilter = filter.Status ?? ApprovalStatus.Pending;

            var entities = await _db.AgentApprovals
                .AsNoTracking()
                .Where(a => a.UserId == userId && a.Status == statusFilter)
                .OrderByDescending(a => a.InsertionDate)
                .Select(a => ToViewModel(a))
                .ToListAsync();

            return BaseResponse<AgentApprovalViewModel>.Ok(entities);
        }

        public async Task<BaseResponse<AgentApprovalViewModel>> ApproveAsync(Guid approvalId, Guid userId)
        {
            var approval = await _db.AgentApprovals
                .FirstOrDefaultAsync(a => a.Id == approvalId && a.UserId == userId);

            if (approval is null)
                return BaseResponse<AgentApprovalViewModel>.Fail(new List<string>() { "Approval not found" });

            approval.Approve();

            await ExecuteToolAsync(approval, userId);

            approval.Status = ApprovalStatus.Approved;
            approval.ResolvedAt = DateTime.UtcNow;
            approval.SetLastModification(DateTime.UtcNow);

            await _db.SaveChangesAsync();

            return BaseResponse<AgentApprovalViewModel>.Ok(ToViewModel(approval));
        }

        public async Task<BaseResponse<AgentApprovalViewModel>> RejectAsync(Guid approvalId, Guid userId)
        {
            var approval = await _db.AgentApprovals
                .FirstOrDefaultAsync(a => a.Id == approvalId && a.UserId == userId);

            if (approval is null)
                return BaseResponse<AgentApprovalViewModel>.Fail(new List<string>() { "Approval not found" });

            approval.Reject();

            approval.Status = ApprovalStatus.Rejected;
            approval.ResolvedAt = DateTime.UtcNow;
            approval.SetLastModification(DateTime.UtcNow);

            await _db.SaveChangesAsync();

            return BaseResponse<AgentApprovalViewModel>.Ok(ToViewModel(approval));
        }

        private async Task ExecuteToolAsync(AgentApproval approval, Guid userId)
        {
            switch (approval.Tool)
            {
                case AgentTools.SaveIncomeExpense:
                case AgentTools.AnalyzeImage:
                case AgentTools.CreateTransaction:
                    await ExecuteCreateTransactionAsync(approval, userId);
                    break;

                case AgentTools.DeleteIncomeExpense:
                case AgentTools.DeleteTransaction:
                    await ExecuteDeleteTransactionAsync(approval, userId);
                    break;

                case AgentTools.ProcessPdfExtract:
                case AgentTools.ProcessBankStatement:
                    await ExecuteProcessBankStatementAsync(approval, userId);
                    break;

                default:
                    throw new InvalidOperationException($"Unknown agent tool: '{approval.Tool}'.");
            }
        }

        private async Task ExecuteCreateTransactionAsync(AgentApproval approval, Guid userId)
        {
            var payload = JsonSerializer.Deserialize<CreateTransactionPayload>(approval.Payload, _jsonOptions)
                ?? throw new InvalidOperationException($"Invalid {approval.Tool} payload.");

            var req = new RegisterTransactionRequest
            {
                BankAccountId = payload.BankAccountId,
                StoreId = payload.StoreId,
                CategoryId = payload.CategoryId,
                CurrencyId = payload.CurrencyId,
                FixedExpenseId = payload.FixedExpenseId,
                OriginType = Enum.Parse<OriginType>(payload.OriginType, ignoreCase: true),
                TransactionType = Enum.Parse<TransactionType>(payload.TransactionType, ignoreCase: true),
                Source = TransactionSource.Agent,
                Amount = payload.Amount,
                Description = payload.Description,
                TransactionDate = payload.TransactionDate.Kind == DateTimeKind.Utc
                    ? payload.TransactionDate
                    : DateTime.SpecifyKind(payload.TransactionDate, DateTimeKind.Utc)
            };

            var transaction = await _transactionHandler.RegisterAsync(req, userId);

            if (payload.Items?.Count > 0)
                await SaveTransactionItemsAsync(transaction.Id, payload.Items);
        }

        private async Task SaveTransactionItemsAsync(Guid transactionId, IEnumerable<TransactionItemPayload> items)
        {
            var now = DateTime.UtcNow;
            foreach (var item in items)
            {
                var txItem = new TransactionItem
                {
                    Id = Guid.NewGuid(),
                    TransactionId = transactionId,
                    Name = item.Name,
                    Quantity = item.Quantity,
                    UnitPrice = item.UnitPrice,
                    TotalPrice = item.TotalPrice,
                    Active = 1
                };
                txItem.SetInsertionDate(now);
                txItem.SetLastModification(now);
                _db.TransactionItems.Add(txItem);
            }
            await _db.SaveChangesAsync();
        }

        private async Task ExecuteDeleteTransactionAsync(AgentApproval approval, Guid userId)
        {
            var payload = JsonSerializer.Deserialize<DeleteTransactionPayload>(approval.Payload, _jsonOptions)
                ?? throw new InvalidOperationException($"Invalid {approval.Tool} payload.");

            await _transactionHandler.DeleteAsync(payload.TransactionId, userId);
        }

        private async Task ExecuteProcessBankStatementAsync(AgentApproval approval, Guid userId)
        {
            var payload = JsonSerializer.Deserialize<ProcessBankStatementPayload>(approval.Payload, _jsonOptions)
                ?? throw new InvalidOperationException($"Invalid {approval.Tool} payload.");

            foreach (var item in payload.Transactions)
            {
                if (!DateTime.TryParse(item.Date, out var date)) continue;
                if (!Enum.TryParse<TransactionType>(item.Type, ignoreCase: true, out var txType)) continue;

                var req = new RegisterTransactionRequest
                {
                    BankAccountId = payload.BankAccountId,
                    StoreId = Guid.Empty,
                    CategoryId = Guid.Empty,
                    CurrencyId = Guid.Empty,
                    FixedExpenseId = null,
                    OriginType = OriginType.Company,
                    TransactionType = txType,
                    Source = TransactionSource.Agent,
                    Amount = item.Amount,
                    Description = item.Description,
                    TransactionDate = DateTime.SpecifyKind(date, DateTimeKind.Utc)
                };

                await _transactionHandler.RegisterAsync(req, userId);
            }
        }

        private static AgentApprovalViewModel ToViewModel(AgentApproval a) => new AgentApprovalViewModel(
            a.Id,
            a.UserId,
            a.AgentName,
            a.Tool,
            a.Payload,
            a.Status.ToString(),
            a.ResolvedAt,
            a.InsertionDate
            );
    }
}
