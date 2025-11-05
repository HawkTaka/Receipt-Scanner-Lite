using ReceiptScannerLite.Data.Models;
using SQLite;

namespace ReceiptScannerLite.Data.Repositories;

public class ReceiptRepository : IReceiptRepository
{
    private readonly AppDb _db;

    public ReceiptRepository(AppDb db)
    {
        _db = db;
    }

    public async Task<int> InsertAsync(Receipt receipt)
    {
        receipt.CreatedUtc = DateTime.UtcNow;
        receipt.UpdatedUtc = DateTime.UtcNow;
        return await _db.Connection.InsertAsync(receipt);
    }

    public async Task UpdateAsync(Receipt receipt)
    {
        receipt.UpdatedUtc = DateTime.UtcNow;
        await _db.Connection.UpdateAsync(receipt);
    }

    public async Task DeleteAsync(int id)
    {
        await _db.Connection.DeleteAsync<Receipt>(id);
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
