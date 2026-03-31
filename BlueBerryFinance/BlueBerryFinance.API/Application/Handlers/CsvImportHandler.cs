using BlueBerryFinance.API.Application.Handlers.Interfaces;
using BlueBerryFinance.API.Data.Context;
using BlueBerryFinance.API.Data.Entities;
using BlueBerryFinance.API.Data.Entities.Enums;
using BlueBerryFinance.Common.ViewModels;
using Microsoft.EntityFrameworkCore;
using System.Globalization;

namespace BlueBerryFinance.API.Application.Handlers
{
    public class CsvImportHandler : ICsvImportHandler
    {
        private readonly AppDbContext _db;
        private readonly ILogger<CsvImportHandler> _logger;

        // Supported date formats from Portuguese banks
        private static readonly string[] _dateFormats =
            ["dd-MM-yyyy", "dd/MM/yyyy", "yyyy-MM-dd", "dd.MM.yyyy"];

        public CsvImportHandler(AppDbContext db, ILogger<CsvImportHandler> logger)
        {
            _db = db;
            _logger = logger;
        }

        public async Task<CsvImportResultViewModel> ImportAsync(
            Guid bankAccountId,
            Guid userId,
            Stream csvStream,
            CancellationToken ct = default)
        {
            var result = new CsvImportResultViewModel();

            // ── Validate bank account ownership ──────────────────────────────────
            var account = await _db.BankAccounts
                .AsNoTracking()
                .FirstOrDefaultAsync(b => b.Id == bankAccountId && b.UserId == userId, ct)
                ?? throw new KeyNotFoundException($"Bank account {bankAccountId} not found.");

            // ── Resolve default category and store ───────────────────────────────
            var (categoryId, storeId) = await EnsureImportDefaultsAsync(userId, ct);

            // ── Load existing transactions for dedup ─────────────────────────────
            var existing = await _db.Transactions
                .AsNoTracking()
                .Where(t => t.BankAccountId == bankAccountId && !t.IsDeleted)
                .Select(t => new { t.TransactionDate, t.Amount, t.Description })
                .ToListAsync(ct);

            var existingSet = existing
                .Select(t => DedupKey(t.TransactionDate, t.Amount, t.Description))
                .ToHashSet();

            // ── Parse CSV rows ───────────────────────────────────────────────────
            var rows = ParseCsv(csvStream, result);

            // ── Map and save ─────────────────────────────────────────────────────
            var toInsert = new List<Transaction>();

            foreach (var row in rows)
            {
                var key = DedupKey(row.Date, Math.Abs(row.Amount), row.Description);
                if (existingSet.Contains(key))
                {
                    result.Skipped++;
                    continue;
                }

                var tx = new Transaction
                {
                    UserId = userId,
                    BankAccountId = bankAccountId,
                    CategoryId = categoryId,
                    StoreId = storeId,
                    CurrencyId = account.CurrencyId,
                    TransactionType = row.Amount >= 0 ? TransactionType.Income : TransactionType.Expense,
                    Amount = Math.Abs(row.Amount),
                    Description = row.Description,
                    TransactionDate = row.Date,
                    Source = TransactionSource.BankSync,
                    OriginType = OriginType.Store,
                    Active = 1
                };

                tx.SetInsertionDate(DateTime.UtcNow);
                tx.SetLastModification(DateTime.UtcNow);

                toInsert.Add(tx);
                existingSet.Add(key); // prevent dupes within the same file
                result.Imported++;
            }

            if (toInsert.Count > 0)
            {
                _db.Transactions.AddRange(toInsert);
                try
                {
                    await _db.SaveChangesAsync(ct);
                }
                catch (DbUpdateException ex)
                {
                    throw new InvalidOperationException("Failed to save imported transactions.", ex);
                }
            }

            _logger.LogInformation(
                "CSV import complete for account {AccountId}: {Imported} imported, {Skipped} skipped, {Errors} errors",
                bankAccountId, result.Imported, result.Skipped, result.Errors);

            return result;
        }

        // ── CSV parser ────────────────────────────────────────────────────────────
        // Supports semicolon or comma delimiters, Portuguese decimal format (comma),
        // and separate Debit/Credit columns OR a single signed Amount column.
        //
        // Detected column headers (case-insensitive, Portuguese + English):
        //   Date:        "Data Mov.", "Data", "Date", "Data Movimento"
        //   Description: "Descrição", "Descricao", "Description", "Histórico"
        //   Debit:       "Débito", "Debito", "Debit", "Montante Débito"
        //   Credit:      "Crédito", "Credito", "Credit", "Montante Crédito"
        //   Amount:      "Valor", "Amount", "Montante"

        private List<CsvRow> ParseCsv(Stream stream, CsvImportResultViewModel result)
        {
            var rows = new List<CsvRow>();
            using var reader = new StreamReader(stream, leaveOpen: true);

            string? headerLine = null;
            char delimiter = ';';
            int[]? colMap = null; // [dateIdx, descIdx, debitIdx, creditIdx, amountIdx]

            int lineNumber = 0;
            while (!reader.EndOfStream)
            {
                var line = reader.ReadLine();
                lineNumber++;
                if (string.IsNullOrWhiteSpace(line)) continue;

                // Detect delimiter on first non-empty line
                if (headerLine is null)
                {
                    delimiter = line.Contains(';') ? ';' : ',';
                    headerLine = line;
                    colMap = DetectColumns(line, delimiter);
                    if (colMap is null)
                    {
                        result.Errors++;
                        result.ErrorMessages.Add("Could not detect CSV column headers. Expected columns: Date, Description, Debit/Credit or Amount.");
                        return rows;
                    }
                    continue;
                }

                var parts = SplitCsvLine(line, delimiter);
                if (parts.Length < 2) continue;

                try
                {
                    var row = MapRow(parts, colMap!);
                    if (row is not null)
                        rows.Add(row);
                    else
                        result.Skipped++;
                }
                catch (Exception ex)
                {
                    result.Errors++;
                    if (result.ErrorMessages.Count < 10)
                        result.ErrorMessages.Add($"Line {lineNumber}: {ex.Message}");
                }
            }

            return rows;
        }

        private static int[]? DetectColumns(string header, char delimiter)
        {
            var cols = SplitCsvLine(header, delimiter)
                .Select((h, i) => (Header: Normalize(h), Index: i))
                .ToList();

            int Find(params string[] names) =>
                cols.FirstOrDefault(c => names.Any(n => c.Header.Contains(n))).Index == 0 &&
                !cols.Any(c => names.Any(n => c.Header.Contains(n)))
                    ? -1
                    : cols.FirstOrDefault(c => names.Any(n => c.Header.Contains(n))).Index;

            var dateIdx   = Find("data mov", "data", "date");
            var descIdx   = Find("descri", "historico", "historico", "description");
            var debitIdx  = Find("debito", "debit", "saida");
            var creditIdx = Find("credito", "credit", "entrada");
            var amountIdx = Find("valor", "amount", "montante", "importancia");

            if (dateIdx < 0 || descIdx < 0) return null;
            if (debitIdx < 0 && creditIdx < 0 && amountIdx < 0) return null;

            return [dateIdx, descIdx, debitIdx, creditIdx, amountIdx];
        }

        private static CsvRow? MapRow(string[] parts, int[] colMap)
        {
            var (dateIdx, descIdx, debitIdx, creditIdx, amountIdx) = (colMap[0], colMap[1], colMap[2], colMap[3], colMap[4]);

            var rawDate = SafeGet(parts, dateIdx);
            var desc    = SafeGet(parts, descIdx).Trim('"', ' ');

            if (string.IsNullOrWhiteSpace(rawDate) || string.IsNullOrWhiteSpace(desc))
                return null;

            if (!TryParseDate(rawDate, out var date))
                throw new FormatException($"Cannot parse date '{rawDate}'.");

            decimal amount;

            if (amountIdx >= 0 && SafeGet(parts, amountIdx) is { Length: > 0 } rawAmount)
            {
                amount = ParseDecimal(rawAmount);
            }
            else
            {
                var debit  = amountIdx < 0 && debitIdx  >= 0 ? ParseDecimal(SafeGet(parts, debitIdx))  : 0m;
                var credit = amountIdx < 0 && creditIdx >= 0 ? ParseDecimal(SafeGet(parts, creditIdx)) : 0m;
                // Debit = money out (negative), Credit = money in (positive)
                amount = credit - debit;
            }

            if (amount == 0) return null;

            return new CsvRow(date, desc, amount);
        }

        // ── Helpers ───────────────────────────────────────────────────────────────

        private async Task<(Guid CategoryId, Guid StoreId)> EnsureImportDefaultsAsync(Guid userId, CancellationToken ct)
        {
            // Find or create a global "Imported" category (not user-scoped — shared)
            var category = await _db.Categories
                .FirstOrDefaultAsync(c => c.Name == "Imported" && !c.IsDeleted, ct);

            if (category is null)
            {
                category = new Category
                {
                    Name = "Imported",
                    Icon = "download",
                    Color = "#9ca3af",
                    Type = "Expense",
                    Active = 1
                };
                category.SetInsertionDate(DateTime.UtcNow);
                category.SetLastModification(DateTime.UtcNow);
                _db.Categories.Add(category);
                await _db.SaveChangesAsync(ct);
            }

            // Find or create a global "Unknown" store
            var store = await _db.Stores
                .FirstOrDefaultAsync(s => s.Name == "Unknown" && !s.IsDeleted, ct);

            if (store is null)
            {
                store = new Store
                {
                    Name = "Unknown",
                    CategoryId = category.Id,
                    Active = 1
                };
                store.SetInsertionDate(DateTime.UtcNow);
                store.SetLastModification(DateTime.UtcNow);
                _db.Stores.Add(store);
                await _db.SaveChangesAsync(ct);
            }

            return (category.Id, store.Id);
        }

        private static string DedupKey(DateTime date, decimal amount, string description) =>
            $"{date:yyyy-MM-dd}|{amount:F2}|{description.ToLowerInvariant().Trim()}";

        private static bool TryParseDate(string raw, out DateTime date)
        {
            raw = raw.Trim('"', ' ');
            return DateTime.TryParseExact(raw, _dateFormats,
                CultureInfo.InvariantCulture, DateTimeStyles.None, out date);
        }

        private static decimal ParseDecimal(string raw)
        {
            raw = raw.Trim('"', ' ', '+');
            if (string.IsNullOrWhiteSpace(raw)) return 0m;
            // Handle Portuguese format: "1.234,56" → "1234.56"
            if (raw.Contains(',') && raw.Contains('.'))
                raw = raw.Replace(".", "").Replace(",", ".");
            else if (raw.Contains(','))
                raw = raw.Replace(",", ".");
            return decimal.TryParse(raw, NumberStyles.Any, CultureInfo.InvariantCulture, out var v) ? v : 0m;
        }

        private static string[] SplitCsvLine(string line, char delimiter)
        {
            // Simple split — handles quoted fields with the delimiter inside
            var result = new List<string>();
            var current = new System.Text.StringBuilder();
            bool inQuotes = false;
            foreach (var ch in line)
            {
                if (ch == '"') { inQuotes = !inQuotes; current.Append(ch); }
                else if (ch == delimiter && !inQuotes) { result.Add(current.ToString()); current.Clear(); }
                else current.Append(ch);
            }
            result.Add(current.ToString());
            return [.. result];
        }

        private static string SafeGet(string[] parts, int idx) =>
            idx >= 0 && idx < parts.Length ? parts[idx] : string.Empty;

        private static string Normalize(string s) =>
            s.Trim('"', ' ').ToLowerInvariant()
             .Replace("ã", "a").Replace("ç", "c").Replace("é", "e")
             .Replace("ê", "e").Replace("á", "a").Replace("ó", "o")
             .Replace("ú", "u").Replace("í", "i").Replace("â", "a");

        private record CsvRow(DateTime Date, string Description, decimal Amount);
    }
}
