using SQLite;

namespace ReceiptScannerLite.Data.Models;

public class Receipt
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }

    public string? StoreName { get; set; }

    public DateTime Date { get; set; }

    public decimal? Subtotal { get; set; }

    public decimal? Tax { get; set; }

    public decimal Total { get; set; }

    public string Category { get; set; } = "Uncategorized";

    public string? Notes { get; set; }

    public string ImagePath { get; set; } = "";

    public string RawText { get; set; } = "";

    public DateTime CreatedUtc { get; set; }

    public DateTime UpdatedUtc { get; set; }
}
