using System.Text;
using ReceiptScannerLite.Data.Models;

namespace ReceiptScannerLite.Services;

public class CsvExportService : ICsvExportService
{
    public async Task ExportReceiptsAsync(IEnumerable<Receipt> receipts, string outputPath)
    {
        var sb = new StringBuilder();

        // Header
        sb.AppendLine("Id,Date,StoreName,Subtotal,Tax,Total,Category,Notes,ImagePath,CreatedUtc,UpdatedUtc");

        // Rows
        foreach (var receipt in receipts)
        {
            sb.AppendLine(
                $"{receipt.Id}," +
                $"{EscapeCsv(receipt.Date.ToString("yyyy-MM-dd"))}," +
                $"{EscapeCsv(receipt.StoreName ?? "")}," +
                $"{receipt.Subtotal?.ToString("F2") ?? ""}," +
                $"{receipt.Tax?.ToString("F2") ?? ""}," +
                $"{receipt.Total:F2}," +
                $"{EscapeCsv(receipt.Category)}," +
                $"{EscapeCsv(receipt.Notes ?? "")}," +
                $"{EscapeCsv(receipt.ImagePath)}," +
                $"{receipt.CreatedUtc:yyyy-MM-dd HH:mm:ss}," +
                $"{receipt.UpdatedUtc:yyyy-MM-dd HH:mm:ss}");
        }

        await File.WriteAllTextAsync(outputPath, sb.ToString());
    }

    public async Task ExportLineItemsAsync(IEnumerable<(LineItem Item, Receipt Receipt)> items, string outputPath)
    {
        var sb = new StringBuilder();

        // Header
        sb.AppendLine("Id,ReceiptId,Date,StoreName,Description,Quantity,UnitPrice,LineTotal,Position");

        // Rows
        foreach (var (item, receipt) in items)
        {
            sb.AppendLine(
                $"{item.Id}," +
                $"{item.ReceiptId}," +
                $"{EscapeCsv(receipt.Date.ToString("yyyy-MM-dd"))}," +
                $"{EscapeCsv(receipt.StoreName ?? "")}," +
                $"{EscapeCsv(item.Description)}," +
                $"{item.Quantity?.ToString("F2") ?? ""}," +
                $"{item.UnitPrice?.ToString("F2") ?? ""}," +
                $"{item.LineTotal?.ToString("F2") ?? ""}," +
                $"{item.Position}");
        }

        await File.WriteAllTextAsync(outputPath, sb.ToString());
    }

    private string EscapeCsv(string? value)
    {
        if (string.IsNullOrEmpty(value))
            return "";

        if (value.Contains(',') || value.Contains('"') || value.Contains('\n') || value.Contains('\r'))
        {
            return $"\"{value.Replace("\"", "\"\"")}\"";
        }

        return value;
    }
}
