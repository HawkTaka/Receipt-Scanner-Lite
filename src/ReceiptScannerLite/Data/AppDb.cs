using SQLite;
using ReceiptScannerLite.Data.Models;

namespace ReceiptScannerLite.Data;

public class AppDb
{
    private readonly SQLiteAsyncConnection _connection;

    public AppDb(string dbPath)
    {
        _connection = new SQLiteAsyncConnection(dbPath);
    }

    public async Task InitializeAsync()
    {
        await _connection.CreateTableAsync<Receipt>();
        await _connection.CreateTableAsync<LineItem>();

        // Create indices
        await _connection.ExecuteAsync("CREATE INDEX IF NOT EXISTS idx_receipt_date ON Receipt(Date)");
        await _connection.ExecuteAsync("CREATE INDEX IF NOT EXISTS idx_receipt_category ON Receipt(Category)");
    }

    public SQLiteAsyncConnection Connection => _connection;
}
