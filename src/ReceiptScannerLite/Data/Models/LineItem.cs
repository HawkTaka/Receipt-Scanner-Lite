using SQLite;

namespace ReceiptScannerLite.Data.Models;

public class LineItem
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }

    [Indexed]
    public int ReceiptId { get; set; }

    public string Description { get; set; } = "";

    public decimal? Quantity { get; set; }

    public decimal? UnitPrice { get; set; }

    public decimal? LineTotal { get; set; }

    public int Position { get; set; }
}
