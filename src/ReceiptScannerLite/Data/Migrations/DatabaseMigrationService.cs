using Microsoft.Extensions.Logging;
using ReceiptScannerLite.Data.Models;
using SQLite;

namespace ReceiptScannerLite.Data.Migrations;

/// <summary>
/// Service for managing database schema migrations
/// </summary>
public class DatabaseMigrationService
{
    private readonly SQLiteAsyncConnection _connection;
    private readonly ILogger<DatabaseMigrationService> _logger;
    private readonly List<IDatabaseMigration> _migrations;

    public DatabaseMigrationService(
        SQLiteAsyncConnection connection,
        ILogger<DatabaseMigrationService> logger)
    {
        _connection = connection;
        _logger = logger;
        _migrations = new List<IDatabaseMigration>();

        // Register all migrations in order
        RegisterMigrations();
    }

    private void RegisterMigrations()
    {
        // Version 1: Initial schema (tables already created by legacy InitializeAsync)
        _migrations.Add(new Migration_001_InitialSchema());

        // Version 2: Add foreign key constraint with CASCADE DELETE
        _migrations.Add(new Migration_002_AddForeignKeyConstraint());

        // Future migrations will be added here in order
        // _migrations.Add(new Migration_003_AddSomeField());
    }

    /// <summary>
    /// Initializes the migration system and applies any pending migrations
    /// </summary>
    public async Task InitializeAsync()
    {
        try
        {
            // Create DbVersion table if it doesn't exist
            await _connection.CreateTableAsync<DbVersion>();

            // Get current version
            var currentVersion = await GetCurrentVersionAsync();
            _logger.LogInformation("Current database version: {Version}", currentVersion);

            // Apply pending migrations
            await ApplyPendingMigrationsAsync(currentVersion);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to initialize database migrations");
            throw;
        }
    }

    /// <summary>
    /// Applies all pending migrations
    /// </summary>
    private async Task ApplyPendingMigrationsAsync(int currentVersion)
    {
        var pendingMigrations = _migrations
            .Where(m => m.Version > currentVersion)
            .OrderBy(m => m.Version)
            .ToList();

        if (!pendingMigrations.Any())
        {
            _logger.LogInformation("No pending migrations");
            return;
        }

        _logger.LogInformation("Applying {Count} pending migrations", pendingMigrations.Count);

        foreach (var migration in pendingMigrations)
        {
            try
            {
                _logger.LogInformation("Applying migration {Version}: {Description}",
                    migration.Version, migration.Description);

                // Execute migration in a transaction
                await _connection.RunInTransactionAsync(async (connection) =>
                {
                    await migration.UpAsync(connection);

                    // Record migration in DbVersion table
                    await connection.InsertAsync(new DbVersion
                    {
                        Version = migration.Version,
                        AppliedUtc = DateTime.UtcNow,
                        Description = migration.Description
                    });
                });

                _logger.LogInformation("Successfully applied migration {Version}", migration.Version);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to apply migration {Version}: {Description}",
                    migration.Version, migration.Description);
                throw new InvalidOperationException(
                    $"Migration {migration.Version} failed: {ex.Message}", ex);
            }
        }

        var newVersion = await GetCurrentVersionAsync();
        _logger.LogInformation("Database upgraded to version {Version}", newVersion);
    }

    /// <summary>
    /// Gets the current database version
    /// </summary>
    private async Task<int> GetCurrentVersionAsync()
    {
        try
        {
            var latestVersion = await _connection.Table<DbVersion>()
                .OrderByDescending(v => v.Version)
                .FirstOrDefaultAsync();

            return latestVersion?.Version ?? 0;
        }
        catch (SQLiteException)
        {
            // DbVersion table doesn't exist yet
            return 0;
        }
    }

    /// <summary>
    /// Gets the migration history
    /// </summary>
    public async Task<IReadOnlyList<DbVersion>> GetMigrationHistoryAsync()
    {
        try
        {
            var history = await _connection.Table<DbVersion>()
                .OrderBy(v => v.Version)
                .ToListAsync();

            return history.AsReadOnly();
        }
        catch (SQLiteException)
        {
            // DbVersion table doesn't exist yet
            return new List<DbVersion>().AsReadOnly();
        }
    }
}
