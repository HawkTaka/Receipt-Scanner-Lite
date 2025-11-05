using ReceiptScannerLite.Data.Models;

namespace ReceiptScannerLite.Data.Repositories;

/// <summary>
/// Repository for managing receipt data persistence
/// </summary>
public interface IReceiptRepository
{
    /// <summary>
    /// Inserts a new receipt into the database
    /// </summary>
    /// <param name="receipt">The receipt to insert</param>
    /// <returns>The ID of the inserted receipt</returns>
    Task<int> InsertAsync(Receipt receipt);

    /// <summary>
    /// Updates an existing receipt in the database
    /// </summary>
    /// <param name="receipt">The receipt to update</param>
    Task UpdateAsync(Receipt receipt);

    /// <summary>
    /// Deletes a receipt from the database
    /// </summary>
    /// <param name="id">The ID of the receipt to delete</param>
    Task DeleteAsync(int id);

    /// <summary>
    /// Gets a single receipt by ID
    /// </summary>
    /// <param name="id">The ID of the receipt to retrieve</param>
    /// <returns>The receipt if found, null otherwise</returns>
    Task<Receipt?> GetAsync(int id);

    /// <summary>
    /// Queries receipts with optional filters
    /// </summary>
    /// <param name="from">Filter by date from (inclusive)</param>
    /// <param name="to">Filter by date to (inclusive)</param>
    /// <param name="category">Filter by category</param>
    /// <param name="storeLike">Filter by store name (partial match)</param>
    /// <returns>List of receipts matching the filters</returns>
    Task<IReadOnlyList<Receipt>> QueryAsync(
        DateTime? from = null,
        DateTime? to = null,
        string? category = null,
        string? storeLike = null);

    /// <summary>
    /// Gets all receipts from the database
    /// </summary>
    /// <returns>List of all receipts</returns>
    Task<IReadOnlyList<Receipt>> GetAllAsync();
}
