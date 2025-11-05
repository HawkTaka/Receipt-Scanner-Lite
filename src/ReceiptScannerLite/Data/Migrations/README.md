# Database Migration System

This directory contains the database migration system for Receipt Scanner Lite. The migration system provides versioned schema upgrades with automatic execution on app startup.

## Overview

The migration system consists of:
- **DbVersion** model: Tracks applied migrations in the database
- **IDatabaseMigration** interface: Contract for all migrations
- **DatabaseMigrationService**: Executes pending migrations
- **Migration_XXX_Description.cs**: Individual migration files

## Creating a New Migration

1. **Determine the version number**: Use the next sequential number (e.g., if version 1 exists, use 2)

2. **Create the migration file**: Copy the template and name it `Migration_XXX_Description.cs`
   ```bash
   cp Migration_002_Example.cs.template Migration_002_AddPaymentMethod.cs
   ```

3. **Implement the migration**:
   ```csharp
   public class Migration_002_AddPaymentMethod : IDatabaseMigration
   {
       public int Version => 2;

       public string Description => "Add PaymentMethod column to Receipt table";

       public async Task UpAsync(SQLiteAsyncConnection connection)
       {
           // Add the column
           await connection.ExecuteAsync(
               "ALTER TABLE Receipt ADD COLUMN PaymentMethod TEXT");

           // Set default value for existing rows
           await connection.ExecuteAsync(
               "UPDATE Receipt SET PaymentMethod = 'Unknown' WHERE PaymentMethod IS NULL");
       }

       public async Task DownAsync(SQLiteAsyncConnection connection)
       {
           // Optional: Implement rollback if needed
           throw new NotSupportedException("Rollback not supported for this migration");
       }
   }
   ```

4. **Register the migration** in `DatabaseMigrationService.RegisterMigrations()`:
   ```csharp
   private void RegisterMigrations()
   {
       _migrations.Add(new Migration_001_InitialSchema());
       _migrations.Add(new Migration_002_AddPaymentMethod());  // Add your migration
   }
   ```

## Migration Best Practices

### Version Numbers
- Use sequential integers starting from 1
- Never reuse or skip version numbers
- Once a migration is deployed, never modify it

### Up Migrations
- Keep migrations small and focused
- Always handle existing data gracefully
- Use transactions for multi-step operations
- Test migrations with existing data

### Down Migrations
- Optional but recommended for development
- Production downgrades are rarely used
- Can throw `NotSupportedException` if not needed

### SQLite Limitations
SQLite has limited ALTER TABLE support:
- ✅ ADD COLUMN is supported
- ❌ DROP COLUMN is not supported
- ❌ ALTER COLUMN is not supported

To work around limitations, use the "recreate table" pattern:
```csharp
// 1. Create new table with desired schema
await connection.ExecuteAsync(@"
    CREATE TABLE Receipt_new (
        Id INTEGER PRIMARY KEY,
        StoreName TEXT,
        -- other columns
    )");

// 2. Copy data from old table
await connection.ExecuteAsync(@"
    INSERT INTO Receipt_new (Id, StoreName, ...)
    SELECT Id, StoreName, ... FROM Receipt");

// 3. Drop old table
await connection.ExecuteAsync("DROP TABLE Receipt");

// 4. Rename new table
await connection.ExecuteAsync("ALTER TABLE Receipt_new RENAME TO Receipt");

// 5. Recreate indices
await connection.ExecuteAsync("CREATE INDEX idx_receipt_date ON Receipt(Date)");
```

## Migration Execution

Migrations are automatically executed on app startup:

1. App starts
2. `AppDb.InitializeAsync()` is called
3. `DatabaseMigrationService` checks current version
4. Pending migrations are applied in order
5. Each migration is wrapped in a transaction
6. Version tracking is updated after each migration

## Viewing Migration History

The migration history is tracked in the `DbVersion` table:
- `Version`: The migration version number
- `AppliedUtc`: When the migration was applied
- `Description`: What the migration does

To view history programmatically:
```csharp
var history = await appDb.MigrationService?.GetMigrationHistoryAsync();
foreach (var version in history)
{
    Console.WriteLine($"Version {version.Version}: {version.Description}");
    Console.WriteLine($"  Applied: {version.AppliedUtc}");
}
```

## Error Handling

If a migration fails:
- The transaction is rolled back
- The app logs the error
- The app throws an exception to prevent startup with corrupted schema
- No subsequent migrations are executed

## Testing Migrations

1. **Test on fresh database**: Delete the app data and run the app
2. **Test on existing database**: Use a copy of production data
3. **Test sequential upgrades**: Verify each migration works in sequence
4. **Test with data**: Ensure existing data is preserved correctly

## Common Scenarios

### Adding a Column
```csharp
await connection.ExecuteAsync(
    "ALTER TABLE Receipt ADD COLUMN NewColumn TEXT");
```

### Creating a Table
```csharp
await connection.CreateTableAsync<NewModel>();
```

### Creating an Index
```csharp
await connection.ExecuteAsync(
    "CREATE INDEX IF NOT EXISTS idx_name ON TableName(ColumnName)");
```

### Migrating Data
```csharp
await connection.ExecuteAsync(
    "UPDATE Receipt SET Category = 'Uncategorized' WHERE Category IS NULL");
```

### Adding a Foreign Key Constraint
Note: SQLite foreign key constraints must be defined at table creation.
```csharp
// Use the "recreate table" pattern shown above
```

## Migration Checklist

Before deploying a migration:
- [ ] Version number is sequential and unique
- [ ] Description is clear and concise
- [ ] Up migration is implemented and tested
- [ ] Down migration is implemented or throws NotSupportedException
- [ ] Migration handles existing data correctly
- [ ] Migration is registered in DatabaseMigrationService
- [ ] Migration is tested on fresh and existing databases
- [ ] Migration is atomic (wrapped in transaction)
- [ ] Migration includes necessary indices
- [ ] Code review completed
