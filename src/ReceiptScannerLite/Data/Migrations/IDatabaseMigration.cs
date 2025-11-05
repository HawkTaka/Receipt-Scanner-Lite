using SQLite;

namespace ReceiptScannerLite.Data.Migrations;

/// <summary>
/// Interface for database schema migrations
/// </summary>
public interface IDatabaseMigration
{
    /// <summary>
    /// The target version number for this migration
    /// </summary>
    int Version { get; }

    /// <summary>
    /// Description of what this migration does
    /// </summary>
    string Description { get; }

    /// <summary>
    /// Applies the migration to upgrade the database
    /// </summary>
    /// <param name="connection">The database connection</param>
    Task UpAsync(SQLiteAsyncConnection connection);

    /// <summary>
    /// Reverts the migration to downgrade the database (optional)
    /// </summary>
    /// <param name="connection">The database connection</param>
    Task DownAsync(SQLiteAsyncConnection connection);
}
