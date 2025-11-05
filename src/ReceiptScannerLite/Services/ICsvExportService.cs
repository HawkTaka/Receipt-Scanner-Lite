using ReceiptScannerLite.Data.Models;

namespace ReceiptScannerLite.Services;

public interface ICsvExportService
{
    /// <summary>
    /// Exports receipts to a CSV file.
    /// </summary>
    /// <param name="receipts">Receipts to export</param>
    /// <param name="outputPath">Output file path</param>
    Task ExportReceiptsAsync(IEnumerable<Receipt> receipts, string outputPath);

    /// <summary>
    /// Exports line items with their associated receipt information to a CSV file.
    /// </summary>
    /// <param name="items">Line items to export (with receipt data)</param>
    /// <param name="outputPath">Output file path</param>
    Task ExportLineItemsAsync(IEnumerable<(LineItem Item, Receipt Receipt)> items, string outputPath);
}
