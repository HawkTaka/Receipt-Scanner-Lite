using ReceiptScannerLite.Data.Models;

namespace ReceiptScannerLite.Data.Repositories;

/// <summary>
/// Repository for managing line item data persistence
/// </summary>
public interface ILineItemRepository
{
    /// <summary>
    /// Bulk inserts or updates line items for a receipt
    /// </summary>
    /// <param name="receiptId">The receipt ID these line items belong to</param>
    /// <param name="items">The line items to upsert</param>
    Task BulkUpsertAsync(int receiptId, IEnumerable<LineItem> items);

    /// <summary>
    /// Gets all line items for a specific receipt
    /// </summary>
    /// <param name="receiptId">The receipt ID to query</param>
    /// <returns>List of line items for the receipt</returns>
    Task<IReadOnlyList<LineItem>> GetByReceiptAsync(int receiptId);

    /// <summary>
    /// Deletes all line items for a receipt
    /// </summary>
    /// <param name="receiptId">The receipt ID whose line items should be deleted</param>
    Task DeleteForReceiptAsync(int receiptId);
}
