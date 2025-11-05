using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ReceiptScannerLite.Data.Models;
using ReceiptScannerLite.Data.Repositories;
using System.Collections.ObjectModel;

namespace ReceiptScannerLite.ViewModels;

public partial class ReceiptDetailViewModel : ObservableObject
{
    private readonly IReceiptRepository _receiptRepository;
    private readonly ILineItemRepository _lineItemRepository;

    [ObservableProperty]
    private Receipt? _receipt;

    [ObservableProperty]
    private ObservableCollection<LineItem> _lineItems = new();

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private bool _showRawText;

    public ReceiptDetailViewModel(
        IReceiptRepository receiptRepository,
        ILineItemRepository lineItemRepository)
    {
        _receiptRepository = receiptRepository;
        _lineItemRepository = lineItemRepository;
    }

    public async Task LoadReceiptAsync(int receiptId)
    {
        IsLoading = true;

        try
        {
            Receipt = await _receiptRepository.GetAsync(receiptId);

            if (Receipt != null)
            {
                var items = await _lineItemRepository.GetByReceiptAsync(receiptId);
                LineItems.Clear();
                foreach (var item in items)
                {
                    LineItems.Add(item);
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading receipt: {ex.Message}");
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private void ToggleRawText()
    {
        ShowRawText = !ShowRawText;
    }

    [RelayCommand]
    private async Task DeleteAsync()
    {
        if (Receipt == null)
            return;

        try
        {
            await _receiptRepository.DeleteAsync(Receipt.Id);
            // Navigate back
            // Shell.Current.GoToAsync("..");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error deleting receipt: {ex.Message}");
        }
    }

    [RelayCommand]
    private void GoBack()
    {
        // Navigate back
        // Shell.Current.GoToAsync("..");
    }
}
