using ReceiptScannerLite.Data.Models;
using SQLite;

namespace ReceiptScannerLite.Data.Migrations;

/// <summary>
/// Migration 002: Add foreign key constraint to LineItem table with CASCADE DELETE
/// </summary>
public class Migration_002_AddForeignKeyConstraint : IDatabaseMigration
{
    public int Version => 2;

    public string Description => "Add foreign key constraint from LineItem.ReceiptId to Receipt.Id with CASCADE DELETE";

    public async Task UpAsync(SQLiteAsyncConnection connection)
    {
        // SQLite doesn't support adding foreign keys to existing tables
        // We need to recreate the LineItem table with the foreign key constraint

        // Step 1: Enable foreign keys (required for this to work)
        await connection.ExecuteAsync("PRAGMA foreign_keys = ON");

        // Step 2: Create new LineItem table with foreign key constraint
        await connection.ExecuteAsync(@"
            CREATE TABLE LineItem_new (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                ReceiptId INTEGER NOT NULL,
                Description TEXT NOT NULL,
                Quantity REAL,
                UnitPrice REAL,
                LineTotal REAL,
                Position INTEGER NOT NULL,
                FOREIGN KEY (ReceiptId) REFERENCES Receipt(Id) ON DELETE CASCADE
            )
        ");

        // Step 3: Copy data from old table to new table
        await connection.ExecuteAsync(@"
            INSERT INTO LineItem_new (Id, ReceiptId, Description, Quantity, UnitPrice, LineTotal, Position)
            SELECT Id, ReceiptId, Description, Quantity, UnitPrice, LineTotal, Position
            FROM LineItem
        ");

        // Step 4: Drop old table
        await connection.ExecuteAsync("DROP TABLE LineItem");

        // Step 5: Rename new table to original name
        await connection.ExecuteAsync("ALTER TABLE LineItem_new RENAME TO LineItem");

        // Step 6: Recreate index on ReceiptId
        await connection.ExecuteAsync(
            "CREATE INDEX IF NOT EXISTS idx_lineitem_receipt ON LineItem(ReceiptId)");
    }

    public async Task DownAsync(SQLiteAsyncConnection connection)
    {
        // To downgrade, we recreate LineItem without the foreign key constraint
        // This is rarely needed in production but good for development

        // Step 1: Create LineItem table without foreign key
        await connection.ExecuteAsync(@"
            CREATE TABLE LineItem_old (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                ReceiptId INTEGER NOT NULL,
                Description TEXT NOT NULL,
                Quantity REAL,
                UnitPrice REAL,
                LineTotal REAL,
                Position INTEGER NOT NULL
            )
        ");

        // Step 2: Copy data
        await connection.ExecuteAsync(@"
            INSERT INTO LineItem_old (Id, ReceiptId, Description, Quantity, UnitPrice, LineTotal, Position)
            SELECT Id, ReceiptId, Description, Quantity, UnitPrice, LineTotal, Position
            FROM LineItem
        ");

        // Step 3: Drop new table
        await connection.ExecuteAsync("DROP TABLE LineItem");

        // Step 4: Rename to original name
        await connection.ExecuteAsync("ALTER TABLE LineItem_old RENAME TO LineItem");

        // Step 5: Recreate index
        await connection.ExecuteAsync(
            "CREATE INDEX IF NOT EXISTS idx_lineitem_receipt ON LineItem(ReceiptId)");
    }
}
