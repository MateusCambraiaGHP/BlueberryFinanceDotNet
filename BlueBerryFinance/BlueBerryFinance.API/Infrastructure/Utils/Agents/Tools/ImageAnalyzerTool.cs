using BlueBerryFinance.API.Data.Context;
using BlueBerryFinance.API.Domain.Entities;
using BlueBerryFinance.API.Domain.Entities.Enums;
using BlueBerryFinance.API.Infrastructure.Services.Interfaces;
using BlueBerryFinance.API.Infrastructure.Utils.Agents.Finance.Interfaces;
using BlueBerryFinance.API.Infrastructure.Utils.Agents.Tools.Interfaces;
using BlueBerryFinance.API.Infrastructure.Utils.Factories.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.AI;
using System.ComponentModel;
using System.Security.Claims;
using System.Text.Json;
using System.Text.Json.Serialization;
using static BlueBerryFinance.API.Infrastructure.Utils.Agents.Tools.AgentWriteTools;

namespace BlueBerryFinance.API.Infrastructure.Utils.Agents.Tools
{
    public partial class ImageAnalyzerTool : IImageAnalyzerTool
    {
        private readonly AppDbContext _db;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IMinioService _minio;
        private readonly IAIAgentFactory _factory;
        private readonly ISecurityValidationAgent _validator;

        private static readonly JsonSerializerOptions _jsonOptions = new()
        {
            Converters = { new JsonStringEnumConverter() },
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        private const string ImageAnalysisPrompt =
            "You are a receipt and invoice analyzer. Extract the following from the image:\n" +
            "- shopName: the store or merchant name\n" +
            "- totalAmount: the final total amount paid (decimal)\n" +
            "- currency: ISO 4217 currency code (e.g. EUR, BRL, USD)\n" +
            "- country: ISO 3166-1 alpha-2 country code (e.g. PT, BR, US)\n" +
            "- description: brief description of the purchase\n" +
            "- transactionDate: date of the transaction in YYYY-MM-DD format\n" +
            "- items: array of individual line items, each with:\n" +
            "    - name: product or item name\n" +
            "    - quantity: quantity purchased (decimal)\n" +
            "    - unitPrice: price per unit (decimal)\n" +
            "    - totalPrice: quantity × unitPrice (decimal)\n\n" +
            "Return ONLY a JSON object with these fields. If items cannot be determined, use an empty array. If any other field cannot be determined, use null.";

        public ImageAnalyzerTool(
            AppDbContext db,
            IHttpContextAccessor httpContextAccessor,
            IMinioService minio,
            IAIAgentFactory factory,
            ISecurityValidationAgent validator)
        {
            _db = db;
            _httpContextAccessor = httpContextAccessor;
            _minio = minio;
            _factory = factory;
            _validator = validator;
        }

        public AITool GetTool() => AIFunctionFactory.Create(
            AnalyzeAsync,
            "analyze_image",
            "Downloads a receipt or invoice image, extracts expense data using vision AI, validates security, and queues a transaction for user approval.");

        private async Task<string> AnalyzeAsync(
            [Description("URL of the uploaded receipt or invoice image as returned by the upload endpoint")] string imageUrl,
            [Description("Bank account name to associate the transaction with. Leave empty to use the default account.")] string? bankAccountName = null)
        {
            var userId = GetCurrentUserId();

            var accountQuery = _db.BankAccounts
                .Where(a => a.UserId == userId && a.Active == 1);
            if (!string.IsNullOrWhiteSpace(bankAccountName))
                accountQuery = accountQuery.Where(a => a.Name.ToLower().Contains(bankAccountName.ToLower()));
            var account = await accountQuery.FirstOrDefaultAsync();
            if (account is null)
                return "No matching bank account found. Please check the account name or create one first.";
            var bankAccountId = account.Id;

            var validationPrompt = $"Validate image analysis: userId={userId}, bankAccountId={bankAccountId}, imageUrl={imageUrl}";
            var validation = await _validator.AskAsync(validationPrompt);

            if (validation is null || !validation.IsValid)
                return $"Security validation failed: {validation?.Reason ?? "Unknown reason"}. Image not processed.";

            var objectName = ExtractObjectName(imageUrl);
            if (objectName is null)
                return $"Invalid image URL format: {imageUrl}";

            await using var imageStream = await _minio.DownloadAsync(objectName);
            using var ms = new MemoryStream();
            await imageStream.CopyToAsync(ms);
            var imageBytes = ms.ToArray();

            var mimeType = DetectMimeType(imageUrl);
            var client = _factory.CreateChatClient();

            var messages = new List<ChatMessage>
            {
                new(ChatRole.System, ImageAnalysisPrompt),
                new(ChatRole.User,
                [
                    new DataContent(imageBytes, mimeType),
                    new TextContent("Extract the receipt data from this image.")
                ])
            };

            ChatResponse response;
            try
            {
                response = await client.GetResponseAsync(messages);
            }
            catch (Exception ex)
            {
                return $"Image analysis failed: {ex.Message}";
            }

            var json = response.Text.Trim();
            if (json.StartsWith("```"))
            {
                var start = json.IndexOf('\n') + 1;
                var end = json.LastIndexOf("```");
                if (end > start) json = json[start..end].Trim();
            }

            ImageReceiptData? receiptData;
            try
            {
                receiptData = JsonSerializer.Deserialize<ImageReceiptData>(json, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
            }
            catch
            {
                return "Could not parse extracted receipt data. Please try again with a clearer image.";
            }

            if (receiptData is null || receiptData.TotalAmount is null)
                return "Could not extract transaction amount from the image. Please try again.";

            // Step 3: Persist FiscalNote record (DB Save Tool)
            var fiscalNote = new FiscalNote
            {
                UserId = userId,
                ImageUrl = imageUrl,
                RawText = string.Empty,
                ExtractedData = json,
                Status = FiscalNoteStatus.Pending
            };
            fiscalNote.SetInsertionDate(DateTime.UtcNow);
            fiscalNote.SetLastModification(DateTime.UtcNow);
            _db.FiscalNotes.Add(fiscalNote);
            await _db.SaveChangesAsync();

            var now = DateTime.UtcNow;

            Guid currencyId = account.CurrencyId;
            if (!string.IsNullOrWhiteSpace(receiptData.Currency))
            {
                var currency = await _db.Currencies.FirstOrDefaultAsync(c =>
                    c.Code.ToString().ToLower() == receiptData.Currency.ToLower() ||
                    c.Symbol.ToLower() == receiptData.Currency.ToLower());
                if (currency is not null) currencyId = currency.Id;
            }

            var category = await _db.Categories.FirstOrDefaultAsync(c => c.Type == "Expense")
                        ?? await _db.Categories.FirstOrDefaultAsync();
            if (category is null)
                return "No categories found. Please create at least one category first.";
            var categoryId = category.Id;

            var shopName = receiptData.ShopName ?? "Unknown Store";
            var store = await _db.Stores.FirstOrDefaultAsync(s =>
                s.Name.ToLower() == shopName.ToLower());
            if (store is null)
            {
                store = new Store
                {
                    Id = Guid.NewGuid(),
                    Name = shopName,
                    CategoryId = categoryId,
                    Active = 1
                };
                store.SetInsertionDate(now);
                store.SetLastModification(now);
                _db.Stores.Add(store);
            }
            var storeId = store.Id;

            // Queue transaction approval (DB Save Tool)
            var description = receiptData.Description ?? shopName;
            var transactionDate = DateTime.TryParse(receiptData.TransactionDate, out var d)
                ? d
                : now;

            var itemPayloads = receiptData.Items?
                .Where(i => !string.IsNullOrWhiteSpace(i.Name))
                .Select(i => new TransactionItemPayload(i.Name, i.Quantity, i.UnitPrice, i.TotalPrice))
                .ToList();

            var payload = new CreateTransactionPayload(
                bankAccountId, storeId, categoryId, currencyId, null,
                "Store", "Expense", receiptData.TotalAmount.Value, description, transactionDate,
                itemPayloads?.Count > 0 ? itemPayloads : null);

            var approval = new AgentApproval
            {
                UserId = userId,
                AgentName = "BlueberryFinanceAgent",
                Tool = "analyze_image",
                Payload = JsonSerializer.Serialize(payload, _jsonOptions),
                Status = ApprovalStatus.Pending,
                Active = 1
            };
            approval.SetInsertionDate(DateTime.UtcNow);
            approval.SetLastModification(DateTime.UtcNow);
            _db.AgentApprovals.Add(approval);
            await _db.SaveChangesAsync();

            return $"Receipt analyzed: {receiptData.ShopName ?? "Unknown store"}, amount {receiptData.TotalAmount} {receiptData.Currency}. " +
                   $"Validated and queued for approval. ApprovalId: {approval.Id}. Approve in the Approvals panel.";
        }

        private static string? ExtractObjectName(string url)
        {
            foreach (var marker in new[] { "statements/", "images/", "receipts/" })
            {
                var idx = url.IndexOf(marker, StringComparison.Ordinal);
                if (idx >= 0) return url[idx..];
            }
            return null;
        }

        private static string DetectMimeType(string url)
        {
            var ext = Path.GetExtension(url).ToLowerInvariant();
            return ext switch
            {
                ".png" => "image/png",
                ".gif" => "image/gif",
                ".webp" => "image/webp",
                _ => "image/jpeg"
            };
        }

        private Guid GetCurrentUserId()
        {
            var claim = _httpContextAccessor.HttpContext?.User.FindFirstValue("userId");
            return Guid.TryParse(claim, out var id) ? id
                : throw new InvalidOperationException("Authenticated user not found in context.");
        }
    }
}
