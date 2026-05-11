using BlueBerryFinance.API.Data.Context;
using BlueBerryFinance.API.Data.Entities;
using BlueBerryFinance.API.Data.Entities.Enums;
using BlueBerryFinance.API.Infrastructure.Utils.Agents.Finance.Interfaces;
using Microsoft.Extensions.AI;
using System.ComponentModel;
using System.Security.Claims;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace BlueBerryFinance.API.Infrastructure.Utils.Agents.Tools
{
    /// <summary>
    /// DB Save Tool — internal write operations.
    /// These tools are NOT registered on the orchestrator agent.
    /// They are used by orchestrator tools (IncomeExpenseSaverTool, ExtractProcessorTool, etc.)
    /// which call the DB directly rather than through this wrapper.
    /// Kept for AgentApprovalHandler payload type sharing and DI compatibility.
    /// </summary>
    public class AgentWriteTools : IAgentWriteTools
    {
        private readonly AppDbContext _db;
        private readonly IHttpContextAccessor _httpContextAccessor;

        private static readonly JsonSerializerOptions _jsonOptions = new()
        {
            Converters = { new JsonStringEnumConverter() },
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        public AgentWriteTools(
            AppDbContext db,
            IHttpContextAccessor httpContextAccessor)
        {
            _db = db;
            _httpContextAccessor = httpContextAccessor;
        }

        /// <summary>
        /// Returns legacy write tools. Not registered on the orchestrator agent.
        /// </summary>
        public IList<AITool> GetTools() =>
        [
            AIFunctionFactory.Create(CreateTransactionAsync, "create_transaction",
                "Legacy: Creates a new financial transaction after user approval."),

            AIFunctionFactory.Create(DeleteTransactionAsync, "delete_transaction",
                "Legacy: Deletes (soft-deletes) a transaction after user approval.")
        ];

        private async Task<string> CreateTransactionAsync(
            [Description("Bank account ID (UUID)")] Guid bankAccountId,
            [Description("Store ID (UUID)")] Guid storeId,
            [Description("Category ID (UUID)")] Guid categoryId,
            [Description("Currency ID (UUID)")] Guid currencyId,
            [Description("Transaction amount as a positive number")] decimal amount,
            [Description("Short description of the transaction")] string description,
            [Description("Transaction date in UTC ISO 8601 format")] DateTime transactionDate,
            [Description("Transaction type. Allowed values: Income, Expense")] string transactionType,
            [Description("Origin type. Allowed values: Person, Company, Store")] string originType,
            [Description("Optional: fixed expense ID")] Guid? fixedExpenseId = null)
        {
            var userId = GetCurrentUserId();

            var payload = new CreateTransactionPayload(
                bankAccountId, storeId, categoryId, currencyId, fixedExpenseId,
                originType, transactionType, amount, description, transactionDate);

            var approval = new AgentApproval
            {
                UserId = userId,
                AgentName = "BlueberryFinanceAgent",
                Tool = "create_transaction",
                Payload = JsonSerializer.Serialize(payload, _jsonOptions),
                Status = ApprovalStatus.Pending,
                Active = 1
            };
            approval.SetInsertionDate(DateTime.UtcNow);
            approval.SetLastModification(DateTime.UtcNow);

            _db.AgentApprovals.Add(approval);
            await _db.SaveChangesAsync();

            return $"Transaction creation queued for user approval. ApprovalId: {approval.Id}.";
        }

        private async Task<string> DeleteTransactionAsync(
            [Description("Transaction ID (UUID) to delete")] Guid transactionId)
        {
            var userId = GetCurrentUserId();

            var payload = new DeleteTransactionPayload(transactionId);

            var approval = new AgentApproval
            {
                UserId = userId,
                AgentName = "BlueberryFinanceAgent",
                Tool = "delete_transaction",
                Payload = JsonSerializer.Serialize(payload, _jsonOptions),
                Status = ApprovalStatus.Pending,
                Active = 1
            };
            approval.SetInsertionDate(DateTime.UtcNow);
            approval.SetLastModification(DateTime.UtcNow);

            _db.AgentApprovals.Add(approval);
            await _db.SaveChangesAsync();

            return $"Transaction deletion queued for user approval. ApprovalId: {approval.Id}.";
        }

        private Guid GetCurrentUserId()
        {
            var claim = _httpContextAccessor.HttpContext?.User.FindFirstValue("userId");
            return Guid.TryParse(claim, out var id) ? id
                : throw new InvalidOperationException("Authenticated user not found in context.");
        }

        public record CreateTransactionPayload(
            Guid BankAccountId,
            Guid StoreId,
            Guid CategoryId,
            Guid CurrencyId,
            Guid? FixedExpenseId,
            string OriginType,
            string TransactionType,
            decimal Amount,
            string Description,
            DateTime TransactionDate,
            IList<TransactionItemPayload>? Items = null);

        public record TransactionItemPayload(
            string Name,
            decimal Quantity,
            decimal UnitPrice,
            decimal TotalPrice);

        public record DeleteTransactionPayload(Guid TransactionId);
    }
}
