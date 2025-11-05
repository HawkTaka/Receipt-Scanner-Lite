using SQLite;

namespace ReceiptScannerLite.Data.Models;

/// <summary>
/// Tracks the current database schema version for migrations
/// </summary>
[Table("DbVersion")]
public class DbVersion
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }

    /// <summary>
    /// The current schema version number
    /// </summary>
    public int Version { get; set; }

    /// <summary>
    /// When this version was applied
    /// </summary>
    public DateTime AppliedUtc { get; set; }

    /// <summary>
    /// Description of what changed in this version
    /// </summary>
    public string? Description { get; set; }
}
