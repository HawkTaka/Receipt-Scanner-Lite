using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ReceiptScannerLite.Data.Models;
using ReceiptScannerLite.Data.Repositories;
using ReceiptScannerLite.Services;
using System.Collections.ObjectModel;

namespace ReceiptScannerLite.ViewModels;

public partial class ReceiptDetailViewModel : ObservableObject
{
    private readonly IReceiptRepository _receiptRepository;
    private readonly ILineItemRepository _lineItemRepository;
    private readonly INavigationService _navigationService;
    private readonly IImageService _imageService;
    private readonly IDialogService _dialogService;

    [ObservableProperty]
    private Receipt? _receipt;

    [ObservableProperty]
    private ObservableCollection<LineItem> _lineItems = new();

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private bool _showRawText;

    [ObservableProperty]
    private string? _imageDataUrl;

    [ObservableProperty]
    private bool _isLoadingImage;

    [ObservableProperty]
    private bool _hasImage;

    public ReceiptDetailViewModel(
        IReceiptRepository receiptRepository,
        ILineItemRepository lineItemRepository,
        INavigationService navigationService,
        IImageService imageService,
        IDialogService dialogService)
    {
        _receiptRepository = receiptRepository;
        _lineItemRepository = lineItemRepository;
        _navigationService = navigationService;
        _imageService = imageService;
        _dialogService = dialogService;
    }

    public async Task LoadReceiptAsync(int receiptId)
    {
        IsLoading = true;

        try
        {
            Receipt = await _receiptRepository.GetAsync(receiptId);

            if (Receipt != null)
            {
                // Load line items
                var items = await _lineItemRepository.GetByReceiptAsync(receiptId);
                LineItems.Clear();
                foreach (var item in items)
                {
                    LineItems.Add(item);
                }

                // Load receipt image
                await LoadImageAsync();
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

    private async Task LoadImageAsync()
    {
        if (Receipt == null || string.IsNullOrWhiteSpace(Receipt.ImagePath))
        {
            HasImage = false;
            ImageDataUrl = null;
            return;
        }

        IsLoadingImage = true;
        try
        {
            ImageDataUrl = await _imageService.GetImageDataUrlAsync(Receipt.ImagePath);
            HasImage = !string.IsNullOrEmpty(ImageDataUrl);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading receipt image: {ex.Message}");
            HasImage = false;
            ImageDataUrl = null;
        }
        finally
        {
            IsLoadingImage = false;
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

        // Show confirmation dialog
        var storeName = string.IsNullOrWhiteSpace(Receipt.StoreName) ? "Unknown Store" : Receipt.StoreName;
        var confirmed = await _dialogService.ConfirmAsync(
            "Delete Receipt",
            $"Are you sure you want to delete the receipt from {storeName} on {Receipt.Date:d}? This action cannot be undone.",
            "Delete",
            "Cancel");

        if (!confirmed)
        {
            return; // User cancelled
        }

        try
        {
            await _receiptRepository.DeleteAsync(Receipt.Id);
            _navigationService.NavigateBack();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error deleting receipt: {ex.Message}");
            await _dialogService.AlertAsync("Delete Failed", $"Failed to delete receipt: {ex.Message}");
        }
    }

    [RelayCommand]
    private void GoBack()
    {
        _navigationService.NavigateBack();
    }
}
