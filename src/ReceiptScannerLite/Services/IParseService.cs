namespace ReceiptScannerLite.Services;

public record ParseResult(
    string? StoreName,
    DateTime? Date,
    decimal? Subtotal,
    decimal? Tax,
    decimal? Total,
    IReadOnlyList<ParsedItem> Items
);

public record ParsedItem(
    string Description,
    decimal? Quantity,
    decimal? UnitPrice,
    decimal? LineTotal
);

public interface IParseService
{
    /// <summary>
    /// Parses raw OCR text to extract receipt information.
    /// </summary>
    /// <param name="rawText">Raw OCR output text</param>
    /// <returns>Parsed receipt data</returns>
    ParseResult Parse(string rawText);
}
