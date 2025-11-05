using ReceiptScannerLite.Data.Models;
using SQLite;

namespace ReceiptScannerLite.Data.Migrations;

/// <summary>
/// Migration 001: Initial database schema with Receipt and LineItem tables
/// </summary>
public class Migration_001_InitialSchema : IDatabaseMigration
{
    public int Version => 1;

    public string Description => "Initial schema with Receipt and LineItem tables";

    public async Task UpAsync(SQLiteAsyncConnection connection)
    {
        // Create tables
        await connection.CreateTableAsync<Receipt>();
        await connection.CreateTableAsync<LineItem>();

        // Create indices for performance
        await connection.ExecuteAsync(
            "CREATE INDEX IF NOT EXISTS idx_receipt_date ON Receipt(Date)");
        await connection.ExecuteAsync(
            "CREATE INDEX IF NOT EXISTS idx_receipt_category ON Receipt(Category)");
        await connection.ExecuteAsync(
            "CREATE INDEX IF NOT EXISTS idx_lineitem_receipt ON LineItem(ReceiptId)");
    }

    public async Task DownAsync(SQLiteAsyncConnection connection)
    {
        // Drop indices
        await connection.ExecuteAsync("DROP INDEX IF EXISTS idx_lineitem_receipt");
        await connection.ExecuteAsync("DROP INDEX IF EXISTS idx_receipt_category");
        await connection.ExecuteAsync("DROP INDEX IF EXISTS idx_receipt_date");

        // Drop tables (this will cascade delete all data!)
        await connection.ExecuteAsync("DROP TABLE IF EXISTS LineItem");
        await connection.ExecuteAsync("DROP TABLE IF EXISTS Receipt");
    }
}
