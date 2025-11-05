using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ReceiptScannerLite.Data.Repositories;
using ReceiptScannerLite.Services;

namespace ReceiptScannerLite.ViewModels;

public partial class ExportViewModel : ObservableObject
{
    private readonly IReceiptRepository _receiptRepository;
    private readonly ILineItemRepository _lineItemRepository;
    private readonly ICsvExportService _csvExportService;

    [ObservableProperty]
    private DateTime? _fromDate;

    [ObservableProperty]
    private DateTime? _toDate;

    [ObservableProperty]
    private bool _isExporting;

    [ObservableProperty]
    private string? _statusMessage;

    [ObservableProperty]
    private int _receiptCount;

    [ObservableProperty]
    private int _lineItemCount;

    public ExportViewModel(
        IReceiptRepository receiptRepository,
        ILineItemRepository lineItemRepository,
        ICsvExportService csvExportService)
    {
        _receiptRepository = receiptRepository;
        _lineItemRepository = lineItemRepository;
        _csvExportService = csvExportService;
    }

    public async Task LoadStatsAsync()
    {
        await UpdateStatsCommand.ExecuteAsync(null);
    }

    [RelayCommand]
    private async Task UpdateStatsAsync()
    {
        try
        {
            var receipts = await _receiptRepository.QueryAsync(
                from: FromDate,
                to: ToDate);

            ReceiptCount = receipts.Count;

            var lineItemCount = 0;
            foreach (var receipt in receipts)
            {
                var items = await _lineItemRepository.GetByReceiptAsync(receipt.Id);
                lineItemCount += items.Count;
            }

            LineItemCount = lineItemCount;
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error: {ex.Message}";
        }
    }

    [RelayCommand]
    private async Task ExportReceiptsAsync()
    {
        IsExporting = true;
        StatusMessage = "Exporting receipts...";

        try
        {
            var receipts = await _receiptRepository.QueryAsync(
                from: FromDate,
                to: ToDate);

            if (!receipts.Any())
            {
                StatusMessage = "No receipts to export.";
                return;
            }

            var outputPath = Path.Combine(
                FileSystem.Current.AppDataDirectory,
                $"receipts_export_{DateTime.Now:yyyyMMdd_HHmmss}.csv");

            await _csvExportService.ExportReceiptsAsync(receipts, outputPath);

            StatusMessage = $"Exported {receipts.Count} receipts to:\n{outputPath}";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Export failed: {ex.Message}";
        }
        finally
        {
            IsExporting = false;
        }
    }

    [RelayCommand]
    private async Task ExportLineItemsAsync()
    {
        IsExporting = true;
        StatusMessage = "Exporting line items...";

        try
        {
            var receipts = await _receiptRepository.QueryAsync(
                from: FromDate,
                to: ToDate);

            if (!receipts.Any())
            {
                StatusMessage = "No receipts to export.";
                return;
            }

            var allItems = new List<(Data.Models.LineItem Item, Data.Models.Receipt Receipt)>();

            foreach (var receipt in receipts)
            {
                var items = await _lineItemRepository.GetByReceiptAsync(receipt.Id);
                foreach (var item in items)
                {
                    allItems.Add((item, receipt));
                }
            }

            if (!allItems.Any())
            {
                StatusMessage = "No line items to export.";
                return;
            }

            var outputPath = Path.Combine(
                FileSystem.Current.AppDataDirectory,
                $"lineitems_export_{DateTime.Now:yyyyMMdd_HHmmss}.csv");

            await _csvExportService.ExportLineItemsAsync(allItems, outputPath);

            StatusMessage = $"Exported {allItems.Count} line items to:\n{outputPath}";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Export failed: {ex.Message}";
        }
        finally
        {
            IsExporting = false;
        }
    }

    partial void OnFromDateChanged(DateTime? value)
    {
        _ = UpdateStatsCommand.ExecuteAsync(null);
    }

    partial void OnToDateChanged(DateTime? value)
    {
        _ = UpdateStatsCommand.ExecuteAsync(null);
    }
}
