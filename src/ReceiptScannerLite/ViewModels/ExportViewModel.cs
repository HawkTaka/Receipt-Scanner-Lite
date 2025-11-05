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

            // Generate filename with timestamp
            var fileName = $"receipts_export_{DateTime.Now:yyyyMMdd_HHmmss}.csv";

            // First export to a temp file
            var tempPath = Path.Combine(FileSystem.Current.CacheDirectory, fileName);
            await _csvExportService.ExportReceiptsAsync(receipts, tempPath);

            // Let user choose save location
            var result = await FileSaver.Default.SaveAsync(fileName, tempPath);

            if (result.IsSuccessful)
            {
                StatusMessage = $"Exported {receipts.Count} receipts to:\n{result.FilePath}";
            }
            else
            {
                StatusMessage = result.Exception?.Message ?? "Export cancelled by user.";
            }

            // Clean up temp file
            try
            {
                if (File.Exists(tempPath))
                {
                    File.Delete(tempPath);
                }
            }
            catch
            {
                // Ignore cleanup errors
            }
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

            // Generate filename with timestamp
            var fileName = $"lineitems_export_{DateTime.Now:yyyyMMdd_HHmmss}.csv";

            // First export to a temp file
            var tempPath = Path.Combine(FileSystem.Current.CacheDirectory, fileName);
            await _csvExportService.ExportLineItemsAsync(allItems, tempPath);

            // Let user choose save location
            var result = await FileSaver.Default.SaveAsync(fileName, tempPath);

            if (result.IsSuccessful)
            {
                StatusMessage = $"Exported {allItems.Count} line items to:\n{result.FilePath}";
            }
            else
            {
                StatusMessage = result.Exception?.Message ?? "Export cancelled by user.";
            }

            // Clean up temp file
            try
            {
                if (File.Exists(tempPath))
                {
                    File.Delete(tempPath);
                }
            }
            catch
            {
                // Ignore cleanup errors
            }
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
