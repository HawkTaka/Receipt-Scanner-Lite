using Microsoft.Extensions.Logging;
using ReceiptScannerLite.Data.Models;
using SQLite;

namespace ReceiptScannerLite.Data.Repositories;

public class ReceiptRepository : IReceiptRepository
{
    private readonly AppDb _db;
    private readonly ILineItemRepository _lineItemRepository;
    private readonly ILogger<ReceiptRepository> _logger;

    public ReceiptRepository(AppDb db, ILineItemRepository lineItemRepository, ILogger<ReceiptRepository> logger)
    {
        _db = db;
        _lineItemRepository = lineItemRepository;
        _logger = logger;
    }

    public async Task<int> InsertAsync(Receipt receipt)
    {
        _logger.LogDebug("Inserting receipt for {StoreName} dated {Date}", receipt.StoreName, receipt.Date);
        receipt.CreatedUtc = DateTime.UtcNow;
        receipt.UpdatedUtc = DateTime.UtcNow;
        var id = await _db.Connection.InsertAsync(receipt);
        _logger.LogInformation("Inserted receipt {ReceiptId} for {StoreName}", id, receipt.StoreName);
        return id;
    }

    public async Task UpdateAsync(Receipt receipt)
    {
        _logger.LogDebug("Updating receipt {ReceiptId}", receipt.Id);
        receipt.UpdatedUtc = DateTime.UtcNow;
        await _db.Connection.UpdateAsync(receipt);
        _logger.LogInformation("Updated receipt {ReceiptId} for {StoreName}", receipt.Id, receipt.StoreName);
    }

    public async Task DeleteAsync(int id)
    {
        _logger.LogDebug("Deleting receipt {ReceiptId}", id);

        // Load receipt to get image path and ensure it exists
        var receipt = await GetAsync(id);
        if (receipt == null)
        {
            _logger.LogWarning("Receipt {ReceiptId} not found for deletion", id);
            return; // Receipt doesn't exist, nothing to delete
        }

        // Delete associated line items first (single SQL statement is more efficient)
        await _lineItemRepository.DeleteForReceiptAsync(id);

        // Delete image file from disk if it exists
        if (!string.IsNullOrWhiteSpace(receipt.ImagePath) && File.Exists(receipt.ImagePath))
        {
            try
            {
                File.Delete(receipt.ImagePath);
            }
            catch (Exception ex)
            {
                // Log but don't fail the delete operation if image cleanup fails
                _logger.LogWarning(ex, "Failed to delete image file {ImagePath}", receipt.ImagePath);
            }
        }

        // Finally, delete the receipt record
        await _db.Connection.DeleteAsync<Receipt>(id);
        _logger.LogInformation("Deleted receipt {ReceiptId} for {StoreName}", id, receipt.StoreName);
    }

    public async Task<Receipt?> GetAsync(int id)
    {
        return await _db.Connection.FindAsync<Receipt>(id);
    }

    public async Task<IReadOnlyList<Receipt>> QueryAsync(
        DateTime? from = null,
        DateTime? to = null,
        string? category = null,
        string? storeLike = null)
    {
        var query = _db.Connection.Table<Receipt>();

        if (from.HasValue)
        {
            query = query.Where(r => r.Date >= from.Value);
        }

        if (to.HasValue)
        {
            query = query.Where(r => r.Date <= to.Value);
        }

        if (!string.IsNullOrWhiteSpace(category))
        {
            query = query.Where(r => r.Category == category);
        }

        if (!string.IsNullOrWhiteSpace(storeLike))
        {
            query = query.Where(r => r.StoreName != null && r.StoreName.Contains(storeLike));
        }

        var results = await query.OrderByDescending(r => r.Date).ToListAsync();
        return results.AsReadOnly();
    }

    public async Task<IReadOnlyList<Receipt>> GetAllAsync()
    {
        var results = await _db.Connection.Table<Receipt>()
            .OrderByDescending(r => r.Date)
            .ToListAsync();
        return results.AsReadOnly();
    }
}
