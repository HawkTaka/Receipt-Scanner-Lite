using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using ReceiptScannerLite.Data.Models;
using ReceiptScannerLite.Data.Repositories;
using ReceiptScannerLite.Services;
using System.Collections.ObjectModel;

namespace ReceiptScannerLite.ViewModels;

public partial class ReceiptsViewModel : ObservableObject
{
    private readonly IReceiptRepository _receiptRepository;
    private readonly INavigationService _navigationService;
    private readonly ICategoryService _categoryService;
    private readonly IDialogService _dialogService;
    private readonly ILogger<ReceiptsViewModel> _logger;
    private readonly IErrorMessageService _errorMessageService;
    private CancellationTokenSource? _searchDebounceTokenSource;

    [ObservableProperty]
    private ObservableCollection<Receipt> _receipts = new();

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private string? _searchText;

    [ObservableProperty]
    private string? _selectedCategory;

    [ObservableProperty]
    private DateTime? _fromDate;

    [ObservableProperty]
    private DateTime? _toDate;

    public IReadOnlyList<string> Categories { get; }

    public ReceiptsViewModel(
        IReceiptRepository receiptRepository,
        INavigationService navigationService,
        ICategoryService categoryService,
        IDialogService dialogService,
        ILogger<ReceiptsViewModel> logger,
        IErrorMessageService errorMessageService)
    {
        _receiptRepository = receiptRepository;
        _navigationService = navigationService;
        _categoryService = categoryService;
        _dialogService = dialogService;
        _logger = logger;
        _errorMessageService = errorMessageService;

        // Add "All" to the beginning of the category list for filtering
        var allCategories = new List<string> { "All" };
        allCategories.AddRange(_categoryService.GetAllCategories());
        Categories = allCategories.AsReadOnly();
    }

    public async Task LoadReceiptsAsync()
    {
        await RefreshCommand.ExecuteAsync(null);
    }

    [RelayCommand]
    private async Task RefreshAsync()
    {
        IsLoading = true;

        try
        {
            var category = SelectedCategory == "All" || string.IsNullOrWhiteSpace(SelectedCategory)
                ? null
                : SelectedCategory;

            var receipts = await _receiptRepository.QueryAsync(
                from: FromDate,
                to: ToDate,
                category: category,
                storeLike: SearchText);

            Receipts.Clear();
            foreach (var receipt in receipts)
            {
                Receipts.Add(receipt);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading receipts");
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private void ClearFilters()
    {
        SearchText = null;
        SelectedCategory = null;
        FromDate = null;
        ToDate = null;
    }

    [RelayCommand]
    private void SetDateRangeToday()
    {
        var today = DateTime.Today;
        FromDate = today;
        ToDate = today;
    }

    [RelayCommand]
    private void SetDateRangeThisWeek()
    {
        var today = DateTime.Today;
        var startOfWeek = today.AddDays(-(int)today.DayOfWeek);
        FromDate = startOfWeek;
        ToDate = today;
    }

    [RelayCommand]
    private void SetDateRangeThisMonth()
    {
        var today = DateTime.Today;
        FromDate = new DateTime(today.Year, today.Month, 1);
        ToDate = today;
    }

    [RelayCommand]
    private void SetDateRangeLast30Days()
    {
        var today = DateTime.Today;
        FromDate = today.AddDays(-30);
        ToDate = today;
    }

    [RelayCommand]
    private void SetDateRangeThisYear()
    {
        var today = DateTime.Today;
        FromDate = new DateTime(today.Year, 1, 1);
        ToDate = today;
    }

    [RelayCommand]
    private void SetDateRangeAllTime()
    {
        FromDate = null;
        ToDate = null;
    }

    [RelayCommand]
    private async Task DeleteReceiptAsync(Receipt receipt)
    {
        // Show confirmation dialog
        var storeName = string.IsNullOrWhiteSpace(receipt.StoreName) ? "Unknown Store" : receipt.StoreName;
        var confirmed = await _dialogService.ConfirmAsync(
            "Delete Receipt",
            $"Are you sure you want to delete the receipt from {storeName} on {receipt.Date:d}? This action cannot be undone.",
            "Delete",
            "Cancel");

        if (!confirmed)
        {
            return; // User cancelled
        }

        try
        {
            await _receiptRepository.DeleteAsync(receipt.Id);
            Receipts.Remove(receipt);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting receipt {ReceiptId}", receipt.Id);
            var errorMessage = _errorMessageService.GetDeleteErrorMessage(ex);
            await _dialogService.AlertAsync("Delete Failed", errorMessage);
        }
    }

    [RelayCommand]
    private void ViewReceipt(Receipt receipt)
    {
        _navigationService.NavigateToReceipt(receipt.Id);
    }

    partial void OnSearchTextChanged(string? value)
    {
        // Cancel any pending search
        _searchDebounceTokenSource?.Cancel();
        _searchDebounceTokenSource?.Dispose();
        _searchDebounceTokenSource = new CancellationTokenSource();

        var token = _searchDebounceTokenSource.Token;

        // Debounce search - wait 400ms before executing
        _ = Task.Run(async () =>
        {
            try
            {
                await Task.Delay(400, token);

                // If not cancelled, execute the search
                if (!token.IsCancellationRequested)
                {
                    await RefreshCommand.ExecuteAsync(null);
                }
            }
            catch (TaskCanceledException)
            {
                // Expected when user types again - do nothing
            }
        }, token);
    }

    partial void OnSelectedCategoryChanged(string? value)
    {
        _ = RefreshCommand.ExecuteAsync(null);
    }

    partial void OnFromDateChanged(DateTime? value)
    {
        _ = RefreshCommand.ExecuteAsync(null);
    }

    partial void OnToDateChanged(DateTime? value)
    {
        _ = RefreshCommand.ExecuteAsync(null);
    }
}
