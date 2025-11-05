using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ReceiptScannerLite.Data.Models;
using ReceiptScannerLite.Data.Repositories;
using ReceiptScannerLite.Services;
using System.Collections.ObjectModel;

namespace ReceiptScannerLite.ViewModels;

public partial class ReceiptsViewModel : ObservableObject
{
    private readonly IReceiptRepository _receiptRepository;
    private readonly INavigationService _navigationService;

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

    public List<string> Categories { get; } = new()
    {
        "All",
        "Uncategorized",
        "Groceries",
        "Dining",
        "Transportation",
        "Entertainment",
        "Shopping",
        "Healthcare",
        "Utilities",
        "Other"
    };

    public ReceiptsViewModel(
        IReceiptRepository receiptRepository,
        INavigationService navigationService)
    {
        _receiptRepository = receiptRepository;
        _navigationService = navigationService;
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
            Console.WriteLine($"Error loading receipts: {ex.Message}");
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
    private async Task DeleteReceiptAsync(Receipt receipt)
    {
        try
        {
            await _receiptRepository.DeleteAsync(receipt.Id);
            Receipts.Remove(receipt);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error deleting receipt: {ex.Message}");
        }
    }

    [RelayCommand]
    private void ViewReceipt(Receipt receipt)
    {
        _navigationService.NavigateToReceipt(receipt.Id);
    }

    partial void OnSearchTextChanged(string? value)
    {
        // Auto-refresh when search text changes (with debounce in real app)
        _ = RefreshCommand.ExecuteAsync(null);
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
