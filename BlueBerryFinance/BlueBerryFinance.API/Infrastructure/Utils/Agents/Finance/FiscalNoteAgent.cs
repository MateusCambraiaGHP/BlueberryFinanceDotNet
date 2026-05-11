using BlueBerryFinance.API.Infrastructure.Utils.Agents.Finance.Interfaces;
using BlueBerryFinance.API.Infrastructure.Utils.Factories.Interfaces;
using BlueBerryFinance.API.Infrastructure.Utils.Helpers.Interfaces;
using Microsoft.Extensions.AI;
using System.Text.Json;
using UglyToad.PdfPig;
using UglyToad.PdfPig.Content;

namespace BlueBerryFinance.API.Infrastructure.Utils.Agents.Finance
{
    public class FiscalNoteAgent : IFiscalNoteAgent
    {
        private readonly IAIAgentFactory _factory;
        private readonly IPromptLoader _prompts;
        private readonly ILogger<FiscalNoteAgent> _logger;

        private static readonly JsonSerializerOptions _jsonOpts = new()
        {
            PropertyNameCaseInsensitive = true
        };

        public FiscalNoteAgent(
            IAIAgentFactory factory,
            IPromptLoader prompts,
            ILogger<FiscalNoteAgent> logger)
        {
            _factory = factory;
            _prompts = prompts;
            _logger = logger;
        }

        public async Task<IReadOnlyList<ExtractedTransaction>> ExtractAsync(
            Stream pdfStream,
            CancellationToken ct = default)
        {
            var rawText = ExtractPdfText(pdfStream);

            if (string.IsNullOrWhiteSpace(rawText))
            {
                _logger.LogWarning("PDF text extraction yielded empty content.");
                return [];
            }

            var systemPrompt = _prompts.Load("Infrastructure.Utils.Agents.Finance.Prompts.FiscalNoteExtractor.md");
            var client = _factory.CreateChatClient();

            var messages = new List<ChatMessage>
            {
                new(ChatRole.System, systemPrompt),
                new(ChatRole.User, rawText)
            };

            var response = await client.GetResponseAsync(messages, cancellationToken: ct);
            var json = response.Text.Trim();

            // Strip optional markdown fences
            if (json.StartsWith("```"))
            {
                var start = json.IndexOf('\n') + 1;
                var end = json.LastIndexOf("```");
                if (end > start) json = json[start..end].Trim();
            }

            try
            {
                var raw = JsonSerializer.Deserialize<List<RawExtractedItem>>(json, _jsonOpts) ?? [];
                return raw
                    .Where(i => i.Date is not null && i.Description is not null)
                    .Select(i => new ExtractedTransaction(
                        DateOnly.Parse(i.Date!),
                        i.Description!.Trim(),
                        Math.Abs(i.Amount),
                        i.Type ?? "Expense"))
                    .ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to parse LLM extraction response: {Json}", json);
                return [];
            }
        }

        private static string ExtractPdfText(Stream stream)
        {
            using var doc = PdfDocument.Open(stream);
            var sb = new System.Text.StringBuilder();

            foreach (Page page in doc.GetPages())
            {
                sb.AppendLine(page.Text);
            }

            return sb.ToString();
        }

        private sealed class RawExtractedItem
        {
            public string? Date { get; set; }
            public string? Description { get; set; }
            public decimal Amount { get; set; }
            public string? Type { get; set; }
        }
    }
}
