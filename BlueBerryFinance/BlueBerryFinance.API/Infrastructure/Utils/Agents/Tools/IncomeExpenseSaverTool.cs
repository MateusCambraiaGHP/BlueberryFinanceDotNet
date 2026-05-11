using BlueBerryFinance.API.Data.Context;
using BlueBerryFinance.API.Data.Entities;
using BlueBerryFinance.API.Data.Entities.Enums;
using BlueBerryFinance.API.Infrastructure.Utils.Agents.Finance.Interfaces;
using BlueBerryFinance.API.Infrastructure.Utils.Agents.Tools.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.AI;
using System.ComponentModel;
using System.Security.Claims;
using System.Text.Json;
using System.Text.Json.Serialization;
using static BlueBerryFinance.API.Infrastructure.Utils.Agents.Tools.AgentWriteTools;

namespace BlueBerryFinance.API.Infrastructure.Utils.Agents.Tools
{
    /// <summary>
    /// Income/Expenses Saver Tool.
    /// Classifies the transaction via ClassificationAgent, validates security via SecurityValidationAgent,
    /// then queues a DB Save approval. Never accesses the database directly for business operations.
    /// </summary>
    public class IncomeExpenseSaverTool : IIncomeExpenseSaverTool
    {
        private readonly AppDbContext _db;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IClassificationAgent _classifier;
        private readonly ISecurityValidationAgent _validator;

        private static readonly JsonSerializerOptions _jsonOptions = new()
        {
            Converters = { new JsonStringEnumConverter() },
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        public IncomeExpenseSaverTool(
            AppDbContext db,
            IHttpContextAccessor httpContextAccessor,
            IClassificationAgent classifier,
            ISecurityValidationAgent validator)
        {
            _db = db;
            _httpContextAccessor = httpContextAccessor;
            _classifier = classifier;
            _validator = validator;
        }

        public IList<AITool> GetTools() =>
        [
            AIFunctionFactory.Create(SaveAsync, "save_income_expense",
                "Classifies and saves a financial income or expense entry after security validation and user approval. Call this when the user wants to register a transaction."),

            AIFunctionFactory.Create(DeleteAsync, "delete_income_expense",
                "Queues a transaction deletion for user approval after security validation. Call this when the user wants to remove a transaction.")
        ];

        private async Task<string> SaveAsync(
            [Description("Transaction amount as a positive number")] decimal amount,
            [Description("Store or merchant name (e.g. 'Pingo Doce', 'Amazon'). Required.")] string storeName,
            [Description("Category name (e.g. 'Groceries', 'Transport'). Leave empty for best match.")] string? categoryName = null,
            [Description("Currency code or symbol (e.g. 'EUR', '€', 'BRL'). Leave empty to use the bank account's currency.")] string? currencyCode = null,
            [Description("Bank account name. Leave empty to use the default account.")] string? bankAccountName = null,
            [Description("Transaction date in ISO 8601 format. Leave empty for today.")] DateTime? transactionDate = null,
            [Description("Optional description of the transaction.")] string? description = null)
        {
            var userId = GetCurrentUserId();
            var now = DateTime.UtcNow;
            var effectiveDate = transactionDate ?? now;

            // Resolve bank account
            var accountQuery = _db.BankAccounts.Where(a => a.UserId == userId && a.Active == 1);
            if (!string.IsNullOrWhiteSpace(bankAccountName))
                accountQuery = accountQuery.Where(a => a.Name.ToLower().Contains(bankAccountName.ToLower()));
            var account = await accountQuery.Include(a => a.Currency).FirstOrDefaultAsync();
            if (account is null)
                return "No matching bank account found. Please check the account name or create one first.";
            var bankAccountId = account.Id;

            // Resolve currency (fall back to account's currency)
            Guid currencyId;
            if (!string.IsNullOrWhiteSpace(currencyCode))
            {
                var currency = await _db.Currencies.FirstOrDefaultAsync(c =>
                    c.Symbol.ToLower() == currencyCode.ToLower() ||
                    c.Code.ToString().ToLower() == currencyCode.ToLower() ||
                    c.Name.ToLower().Contains(currencyCode.ToLower()));
                if (currency is null)
                    return $"Currency '{currencyCode}' not found. Please use a valid currency code (e.g. EUR, BRL).";
                currencyId = currency.Id;
            }
            else
            {
                currencyId = account.CurrencyId;
            }

            // Resolve category
            Guid categoryId;
            var categoryQuery = _db.Categories.AsQueryable();
            if (!string.IsNullOrWhiteSpace(categoryName))
                categoryQuery = categoryQuery.Where(c => c.Name.ToLower().Contains(categoryName.ToLower()));
            var category = await categoryQuery.FirstOrDefaultAsync()
                        ?? await _db.Categories.FirstOrDefaultAsync(c => c.Type == "Expense")
                        ?? await _db.Categories.FirstOrDefaultAsync();
            if (category is null)
                return "No categories found. Please create at least one category first.";
            categoryId = category.Id;

            // Resolve or create store
            var store = await _db.Stores.FirstOrDefaultAsync(s =>
                s.Name.ToLower() == storeName.ToLower());
            if (store is null)
            {
                store = new Store
                {
                    Id = Guid.NewGuid(),
                    Name = storeName,
                    CategoryId = categoryId,
                    Active = 1
                };
                store.SetInsertionDate(now);
                store.SetLastModification(now);
                _db.Stores.Add(store);
            }
            var storeId = store.Id;

            var effectiveDescription = string.IsNullOrWhiteSpace(description) ? storeName : description;

            // Step 1: Classify
            var classificationPrompt = $"Classify this transaction: amount={amount}, description=\"{effectiveDescription}\", store=\"{storeName}\", date={effectiveDate:yyyy-MM-dd}";
            var classification = await _classifier.AskAsync(classificationPrompt);

            if (classification is null)
                return "Classification failed. Please try again.";

            // Step 2: Security validation (uses DB Query Tool internally)
            var validationPrompt = $"Validate: userId={userId}, bankAccountId={bankAccountId}, storeId={storeId}, " +
                                   $"categoryId={categoryId}, currencyId={currencyId}, amount={amount}, " +
                                   $"type={classification.Type}, originType={classification.OriginType}";
            var validation = await _validator.AskAsync(validationPrompt);

            if (validation is null || !validation.IsValid)
                return $"Security validation failed: {validation?.Reason ?? "Unknown reason"}. Transaction not queued.";

            // Step 3: Queue for DB Save (approval required)
            var payload = new CreateTransactionPayload(
                bankAccountId, storeId, categoryId, currencyId, null,
                classification.OriginType, classification.Type, amount,
                string.IsNullOrWhiteSpace(classification.Description) ? effectiveDescription : classification.Description,
                effectiveDate);

            var approval = new AgentApproval
            {
                UserId = userId,
                AgentName = "BlueberryFinanceAgent",
                Tool = "save_income_expense",
                Payload = JsonSerializer.Serialize(payload, _jsonOptions),
                Status = ApprovalStatus.Pending,
                Active = 1
            };
            approval.SetInsertionDate(DateTime.UtcNow);
            approval.SetLastModification(DateTime.UtcNow);

            _db.AgentApprovals.Add(approval);
            await _db.SaveChangesAsync();

            return $"Transaction classified as {classification.Type} ({classification.OriginType}) — \"{classification.CategorySuggestion}\". " +
                   $"Validated and queued for approval. ApprovalId: {approval.Id}. Approve in the Approvals panel.";
        }

        private async Task<string> DeleteAsync(
            [Description("Transaction ID (UUID) to delete")] Guid transactionId)
        {
            var userId = GetCurrentUserId();

            var validationPrompt = $"Validate deletion: userId={userId}, transactionId={transactionId}";
            var validation = await _validator.AskAsync(validationPrompt);

            if (validation is null || !validation.IsValid)
                return $"Security validation failed: {validation?.Reason ?? "Unknown reason"}. Deletion not queued.";

            var payload = new DeleteTransactionPayload(transactionId);

            var approval = new AgentApproval
            {
                UserId = userId,
                AgentName = "BlueberryFinanceAgent",
                Tool = "delete_income_expense",
                Payload = JsonSerializer.Serialize(payload, _jsonOptions),
                Status = ApprovalStatus.Pending,
                Active = 1
            };
            approval.SetInsertionDate(DateTime.UtcNow);
            approval.SetLastModification(DateTime.UtcNow);

            _db.AgentApprovals.Add(approval);
            await _db.SaveChangesAsync();

            return $"Transaction deletion validated and queued for approval. ApprovalId: {approval.Id}. Approve in the Approvals panel.";
        }

        private Guid GetCurrentUserId()
        {
            var claim = _httpContextAccessor.HttpContext?.User.FindFirstValue("userId");
            return Guid.TryParse(claim, out var id) ? id
                : throw new InvalidOperationException("Authenticated user not found in context.");
        }
    }
}
