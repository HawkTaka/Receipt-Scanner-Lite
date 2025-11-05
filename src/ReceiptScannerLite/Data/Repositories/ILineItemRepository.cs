using ReceiptScannerLite.Data.Models;

namespace ReceiptScannerLite.Data.Repositories;

public interface ILineItemRepository
{
    Task BulkUpsertAsync(int receiptId, IEnumerable<LineItem> items);
    Task<IReadOnlyList<LineItem>> GetByReceiptAsync(int receiptId);
    Task DeleteForReceiptAsync(int receiptId);
}
