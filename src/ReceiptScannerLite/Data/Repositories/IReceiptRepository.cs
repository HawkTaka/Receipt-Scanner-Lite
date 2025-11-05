using ReceiptScannerLite.Data.Models;

namespace ReceiptScannerLite.Data.Repositories;

public interface IReceiptRepository
{
    Task<int> InsertAsync(Receipt receipt);
    Task UpdateAsync(Receipt receipt);
    Task DeleteAsync(int id);
    Task<Receipt?> GetAsync(int id);
    Task<IReadOnlyList<Receipt>> QueryAsync(
        DateTime? from = null,
        DateTime? to = null,
        string? category = null,
        string? storeLike = null);
    Task<IReadOnlyList<Receipt>> GetAllAsync();
}
