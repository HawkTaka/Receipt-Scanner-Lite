using ReceiptScannerLite.Data.Models;

namespace ReceiptScannerLite.Data.Repositories;

public class LineItemRepository : ILineItemRepository
{
    private readonly AppDb _db;

    public LineItemRepository(AppDb db)
    {
        _db = db;
    }

    public async Task BulkUpsertAsync(int receiptId, IEnumerable<LineItem> items)
    {
        // Delete existing items for this receipt
        await DeleteForReceiptAsync(receiptId);

        // Insert new items
        var itemsList = items.ToList();
        for (int i = 0; i < itemsList.Count; i++)
        {
            var item = itemsList[i];
            item.ReceiptId = receiptId;
            item.Position = i;
            await _db.Connection.InsertAsync(item);
        }
    }

    public async Task<IReadOnlyList<LineItem>> GetByReceiptAsync(int receiptId)
    {
        var results = await _db.Connection.Table<LineItem>()
            .Where(li => li.ReceiptId == receiptId)
            .OrderBy(li => li.Position)
            .ToListAsync();
        return results.AsReadOnly();
    }

    public async Task DeleteForReceiptAsync(int receiptId)
    {
        await _db.Connection.ExecuteAsync("DELETE FROM LineItem WHERE ReceiptId = ?", receiptId);
    }
}
