using Microsoft.Extensions.Logging;
using ReceiptScannerLite.Data.Models;

namespace ReceiptScannerLite.Data.Repositories;

public class LineItemRepository : ILineItemRepository
{
    private readonly AppDb _db;
    private readonly ILogger<LineItemRepository> _logger;

    public LineItemRepository(AppDb db, ILogger<LineItemRepository> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task BulkUpsertAsync(int receiptId, IEnumerable<LineItem> items)
    {
        var itemsList = items.ToList();
        _logger.LogDebug("Bulk upserting {Count} line items for receipt {ReceiptId}", itemsList.Count, receiptId);

        await _db.Connection.RunInTransactionAsync((connection) =>
        {
            // Delete existing items for this receipt
            connection.Execute("DELETE FROM LineItem WHERE ReceiptId = ?", receiptId);

            // Prepare new items with ReceiptId and Position
            for (int i = 0; i < itemsList.Count; i++)
            {
                itemsList[i].ReceiptId = receiptId;
                itemsList[i].Position = i;
            }

            // Insert all items in a single batch operation
            if (itemsList.Count > 0)
            {
                connection.InsertAll(itemsList);
            }
        });

        _logger.LogInformation("Upserted {Count} line items for receipt {ReceiptId}", itemsList.Count, receiptId);
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
