using System.Globalization;
using System.Text.RegularExpressions;

namespace ReceiptScannerLite.Services;

public partial class ParseService : IParseService
{
    // Date patterns
    [GeneratedRegex(@"\b(0?[1-9]|[12][0-9]|3[01])[-/ .](0?[1-9]|1[0-2])[-/ .](\d{2,4})\b", RegexOptions.IgnoreCase)]
    private static partial Regex DateDDMMYYYYRegex();

    [GeneratedRegex(@"\b\d{4}[-/](0?[1-9]|1[0-2])[-/](0?[1-9]|[12][0-9]|3[01])\b", RegexOptions.IgnoreCase)]
    private static partial Regex DateYYYYMMDDRegex();

    [GeneratedRegex(@"\b(0?[1-9]|1[0-2])[-/ .](0?[1-9]|[12][0-9]|3[01])[-/ .](\d{2,4})\b", RegexOptions.IgnoreCase)]
    private static partial Regex DateMMDDYYYYRegex();

    // Money pattern: supports 1,234.56 or 1 234,56 formats
    [GeneratedRegex(@"(?<!\d)(\d{1,3}(?:[.,\s]\d{3})*(?:[.,]\d{2})?)(?!\d)", RegexOptions.IgnoreCase)]
    private static partial Regex MoneyRegex();

    // Total keywords
    [GeneratedRegex(@"\b(TOTAL|GRAND\s*TOTAL|AMOUNT\s*DUE|BALANCE\s*DUE)\b", RegexOptions.IgnoreCase)]
    private static partial Regex TotalKeywordRegex();

    // Tax/VAT keywords
    [GeneratedRegex(@"\b(VAT|TAX|GST)\b", RegexOptions.IgnoreCase)]
    private static partial Regex TaxKeywordRegex();

    // Subtotal keywords
    [GeneratedRegex(@"\b(SUBTOTAL|SUB\s*TOTAL|SUB-TOTAL)\b", RegexOptions.IgnoreCase)]
    private static partial Regex SubtotalKeywordRegex();

    // Quantity x Price pattern: e.g., "2 x 5.99" or "2x5.99"
    [GeneratedRegex(@"(\d+(?:\.\d+)?)\s*[xX*×]\s*(\d+(?:[.,]\d{2})?)", RegexOptions.IgnoreCase)]
    private static partial Regex QuantityPriceRegex();

    public ParseResult Parse(string rawText)
    {
        if (string.IsNullOrWhiteSpace(rawText))
        {
            return new ParseResult(null, null, null, null, null, Array.Empty<ParsedItem>());
        }

        var lines = rawText.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries)
            .Select(l => l.Trim())
            .Where(l => !string.IsNullOrWhiteSpace(l))
            .ToList();

        var storeName = ExtractStoreName(lines);
        var date = ExtractDate(lines);
        var (subtotal, tax, total) = ExtractFinancials(lines);
        var items = ExtractLineItems(lines);

        return new ParseResult(storeName, date, subtotal, tax, total, items);
    }

    private string? ExtractStoreName(List<string> lines)
    {
        // Look at the first few lines for store name
        // Store name is usually all caps or capitalized, and not a date/address/total
        for (int i = 0; i < Math.Min(5, lines.Count); i++)
        {
            var line = lines[i];

            // Skip if it looks like a date
            if (DateDDMMYYYYRegex().IsMatch(line) ||
                DateYYYYMMDDRegex().IsMatch(line) ||
                DateMMDDYYYYRegex().IsMatch(line))
                continue;

            // Skip if it contains total/tax keywords
            if (TotalKeywordRegex().IsMatch(line) || TaxKeywordRegex().IsMatch(line))
                continue;

            // Skip if it looks like an address (contains numbers and common address words)
            if (Regex.IsMatch(line, @"\b\d+\s+(street|st|road|rd|avenue|ave|drive|dr|lane|ln)\b", RegexOptions.IgnoreCase))
                continue;

            // Prefer lines that are mostly uppercase or properly capitalized
            if (line.Length > 3 && (line == line.ToUpper() || char.IsUpper(line[0])))
            {
                return line;
            }
        }

        return null;
    }

    private DateTime? ExtractDate(List<string> lines)
    {
        foreach (var line in lines)
        {
            // Try different date formats
            var match = DateYYYYMMDDRegex().Match(line);
            if (match.Success && TryParseDate(match.Value, out var date1))
                return date1;

            match = DateDDMMYYYYRegex().Match(line);
            if (match.Success && TryParseDate(match.Value, out var date2))
                return date2;

            match = DateMMDDYYYYRegex().Match(line);
            if (match.Success && TryParseDate(match.Value, out var date3))
                return date3;
        }

        return null;
    }

    private bool TryParseDate(string dateStr, out DateTime date)
    {
        var formats = new[]
        {
            "yyyy-MM-dd", "yyyy/MM/dd",
            "dd-MM-yyyy", "dd/MM/yyyy", "dd.MM.yyyy",
            "MM-dd-yyyy", "MM/dd/yyyy", "MM.dd.yyyy",
            "dd-MM-yy", "dd/MM/yy", "dd.MM.yy",
            "MM-dd-yy", "MM/dd/yy", "MM.dd.yy"
        };

        return DateTime.TryParseExact(
            dateStr,
            formats,
            CultureInfo.InvariantCulture,
            DateTimeStyles.None,
            out date);
    }

    private (decimal? subtotal, decimal? tax, decimal? total) ExtractFinancials(List<string> lines)
    {
        decimal? subtotal = null;
        decimal? tax = null;
        decimal? total = null;

        // Look for labeled amounts
        for (int i = 0; i < lines.Count; i++)
        {
            var line = lines[i];

            // Check for total
            if (total == null && TotalKeywordRegex().IsMatch(line))
            {
                var money = MoneyRegex().Matches(line);
                if (money.Count > 0)
                {
                    var lastMatch = money[money.Count - 1];
                    if (TryNormalizeMoney(lastMatch.Value, out var amount))
                        total = amount;
                }
            }

            // Check for tax
            if (tax == null && TaxKeywordRegex().IsMatch(line))
            {
                var money = MoneyRegex().Matches(line);
                if (money.Count > 0)
                {
                    var lastMatch = money[money.Count - 1];
                    if (TryNormalizeMoney(lastMatch.Value, out var amount))
                        tax = amount;
                }
            }

            // Check for subtotal
            if (subtotal == null && SubtotalKeywordRegex().IsMatch(line))
            {
                var money = MoneyRegex().Matches(line);
                if (money.Count > 0)
                {
                    var lastMatch = money[money.Count - 1];
                    if (TryNormalizeMoney(lastMatch.Value, out var amount))
                        subtotal = amount;
                }
            }
        }

        // If total not found, look for the largest amount in the bottom third
        if (total == null)
        {
            var bottomThird = lines.Skip(lines.Count * 2 / 3).ToList();
            var amounts = new List<decimal>();

            foreach (var line in bottomThird)
            {
                var matches = MoneyRegex().Matches(line);
                foreach (Match match in matches)
                {
                    if (TryNormalizeMoney(match.Value, out var amount))
                        amounts.Add(amount);
                }
            }

            if (amounts.Any())
                total = amounts.Max();
        }

        return (subtotal, tax, total);
    }

    private IReadOnlyList<ParsedItem> ExtractLineItems(List<string> lines)
    {
        var items = new List<ParsedItem>();

        // Simple heuristic: lines with money amounts that aren't in keywords
        foreach (var line in lines)
        {
            // Skip lines with total/tax/subtotal keywords
            if (TotalKeywordRegex().IsMatch(line) ||
                TaxKeywordRegex().IsMatch(line) ||
                SubtotalKeywordRegex().IsMatch(line))
                continue;

            var moneyMatches = MoneyRegex().Matches(line);
            if (moneyMatches.Count == 0)
                continue;

            // Extract line total (last money value)
            var lastMoneyMatch = moneyMatches[moneyMatches.Count - 1];
            if (!TryNormalizeMoney(lastMoneyMatch.Value, out var lineTotal))
                continue;

            // Extract description (everything before the last money value)
            var description = line.Substring(0, lastMoneyMatch.Index).Trim();
            if (string.IsNullOrWhiteSpace(description))
                continue;

            // Try to extract quantity and unit price
            decimal? quantity = null;
            decimal? unitPrice = null;

            var qtyPriceMatch = QuantityPriceRegex().Match(line);
            if (qtyPriceMatch.Success)
            {
                if (decimal.TryParse(qtyPriceMatch.Groups[1].Value, NumberStyles.Any, CultureInfo.InvariantCulture, out var qty))
                    quantity = qty;

                if (TryNormalizeMoney(qtyPriceMatch.Groups[2].Value, out var price))
                    unitPrice = price;
            }

            items.Add(new ParsedItem(description, quantity, unitPrice, lineTotal));
        }

        return items.AsReadOnly();
    }

    public bool TryNormalizeMoney(string input, out decimal result)
    {
        result = 0;
        if (string.IsNullOrWhiteSpace(input))
            return false;

        // Remove spaces
        var cleaned = input.Replace(" ", "");

        // Determine decimal separator
        var lastComma = cleaned.LastIndexOf(',');
        var lastDot = cleaned.LastIndexOf('.');

        // If both present, the later one is likely the decimal separator
        if (lastComma >= 0 && lastDot >= 0)
        {
            if (lastComma > lastDot)
            {
                // European format: 1.234,56
                cleaned = cleaned.Replace(".", "").Replace(",", ".");
            }
            else
            {
                // US format: 1,234.56
                cleaned = cleaned.Replace(",", "");
            }
        }
        else if (lastComma >= 0)
        {
            // Only comma present
            // If comma appears near the end (last 3 chars), it's likely decimal
            if (cleaned.Length - lastComma <= 3)
                cleaned = cleaned.Replace(",", ".");
            else
                cleaned = cleaned.Replace(",", "");
        }
        // If only dot present, leave as is

        return decimal.TryParse(cleaned, NumberStyles.Any, CultureInfo.InvariantCulture, out result);
    }
}
