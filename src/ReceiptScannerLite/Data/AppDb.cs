using Microsoft.Extensions.Logging;
using ReceiptScannerLite.Data.Migrations;
using SQLite;

namespace ReceiptScannerLite.Data;

public class AppDb
{
    private readonly SQLiteAsyncConnection _connection;
    private readonly ILogger<DatabaseMigrationService> _migrationLogger;
    private DatabaseMigrationService? _migrationService;

    public AppDb(string dbPath, ILogger<DatabaseMigrationService> migrationLogger)
    {
        _connection = new SQLiteAsyncConnection(dbPath);
        _migrationLogger = migrationLogger;
    }

    public async Task InitializeAsync()
    {
        // Use migration service to handle schema initialization and upgrades
        _migrationService = new DatabaseMigrationService(_connection, _migrationLogger);
        await _migrationService.InitializeAsync();
    }

    public SQLiteAsyncConnection Connection => _connection;

    public DatabaseMigrationService? MigrationService => _migrationService;
}
