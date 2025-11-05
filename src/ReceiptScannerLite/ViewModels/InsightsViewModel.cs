using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using ReceiptScannerLite.Data.Repositories;
using System.Collections.ObjectModel;

namespace ReceiptScannerLite.ViewModels;

public partial class InsightsViewModel : ObservableObject
{
    private readonly IReceiptRepository _receiptRepository;
    private readonly ILogger<InsightsViewModel> _logger;

    [ObservableProperty]
    private ObservableCollection<MonthlyTotal> _monthlyTotals = new();

    [ObservableProperty]
    private ObservableCollection<CategoryBreakdown> _categoryBreakdowns = new();

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private decimal _grandTotal;

    [ObservableProperty]
    private int _receiptCount;

    public InsightsViewModel(IReceiptRepository receiptRepository, ILogger<InsightsViewModel> logger)
    {
        _receiptRepository = receiptRepository;
        _logger = logger;
    }

    public async Task LoadInsightsAsync()
    {
        await RefreshCommand.ExecuteAsync(null);
    }

    [RelayCommand]
    private async Task RefreshAsync()
    {
        IsLoading = true;

        try
        {
            var allReceipts = await _receiptRepository.GetAllAsync();

            // Calculate monthly totals
            var monthlyData = allReceipts
                .GroupBy(r => new { r.Date.Year, r.Date.Month })
                .Select(g => new MonthlyTotal
                {
                    Year = g.Key.Year,
                    Month = g.Key.Month,
                    MonthName = new DateTime(g.Key.Year, g.Key.Month, 1).ToString("MMMM yyyy"),
                    Total = g.Sum(r => r.Total),
                    Count = g.Count()
                })
                .OrderByDescending(m => m.Year)
                .ThenByDescending(m => m.Month)
                .ToList();

            MonthlyTotals.Clear();
            foreach (var item in monthlyData)
            {
                MonthlyTotals.Add(item);
            }

            // Calculate category breakdown
            var categoryData = allReceipts
                .GroupBy(r => r.Category)
                .Select(g => new CategoryBreakdown
                {
                    Category = g.Key,
                    Total = g.Sum(r => r.Total),
                    Count = g.Count(),
                    Percentage = 0 // Will calculate after
                })
                .OrderByDescending(c => c.Total)
                .ToList();

            var totalAmount = categoryData.Sum(c => c.Total);
            if (totalAmount > 0)
            {
                foreach (var cat in categoryData)
                {
                    cat.Percentage = (double)(cat.Total / totalAmount * 100);
                }
            }

            CategoryBreakdowns.Clear();
            foreach (var item in categoryData)
            {
                CategoryBreakdowns.Add(item);
            }

            // Overall stats
            GrandTotal = allReceipts.Sum(r => r.Total);
            ReceiptCount = allReceipts.Count;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading insights");
        }
        finally
        {
            IsLoading = false;
        }
    }
}

public class MonthlyTotal : ObservableObject
{
    public int Year { get; set; }
    public int Month { get; set; }
    public string MonthName { get; set; } = "";
    public decimal Total { get; set; }
    public int Count { get; set; }
}

public class CategoryBreakdown : ObservableObject
{
    public string Category { get; set; } = "";
    public decimal Total { get; set; }
    public int Count { get; set; }
    public double Percentage { get; set; }
}
