using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ReceiptScannerLite.Data.Models;
using ReceiptScannerLite.Data.Repositories;
using System.Collections.ObjectModel;

namespace ReceiptScannerLite.ViewModels;

public partial class ReviewViewModel : ObservableObject
{
    private readonly IReceiptRepository _receiptRepository;
    private readonly ILineItemRepository _lineItemRepository;

    [ObservableProperty]
    private string? _storeName;

    [ObservableProperty]
    private DateTime _date = DateTime.Today;

    [ObservableProperty]
    private decimal? _subtotal;

    [ObservableProperty]
    private decimal? _tax;

    [ObservableProperty]
    private decimal? _total;

    [ObservableProperty]
    private string _category = "Uncategorized";

    [ObservableProperty]
    private string? _notes;

    [ObservableProperty]
    private string _imagePath = "";

    [ObservableProperty]
    private string _rawText = "";

    [ObservableProperty]
    private bool _isSaving;

    [ObservableProperty]
    private string? _statusMessage;

    public ObservableCollection<LineItemEdit> LineItems { get; } = new();

    public List<string> Categories { get; } = new()
    {
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

    public ReviewViewModel(
        IReceiptRepository receiptRepository,
        ILineItemRepository lineItemRepository)
    {
        _receiptRepository = receiptRepository;
        _lineItemRepository = lineItemRepository;
    }

    public void LoadParseResult(string imagePath, string rawText, Services.ParseResult? parseResult)
    {
        ImagePath = imagePath;
        RawText = rawText;

        if (parseResult == null)
            return;

        StoreName = parseResult.StoreName;
        Date = parseResult.Date ?? DateTime.Today;
        Subtotal = parseResult.Subtotal;
        Tax = parseResult.Tax;
        Total = parseResult.Total;

        LineItems.Clear();
        foreach (var item in parseResult.Items)
        {
            LineItems.Add(new LineItemEdit
            {
                Description = item.Description,
                Quantity = item.Quantity,
                UnitPrice = item.UnitPrice,
                LineTotal = item.LineTotal
            });
        }
    }

    [RelayCommand]
    private void AddLineItem()
    {
        LineItems.Add(new LineItemEdit
        {
            Description = "New Item",
            LineTotal = 0
        });
    }

    [RelayCommand]
    private void RemoveLineItem(LineItemEdit item)
    {
        LineItems.Remove(item);
    }

    [RelayCommand]
    private async Task SaveAsync()
    {
        if (!Total.HasValue || Total.Value <= 0)
        {
            StatusMessage = "Please enter a valid total amount.";
            return;
        }

        IsSaving = true;
        StatusMessage = "Saving receipt...";

        try
        {
            // Create receipt
            var receipt = new Receipt
            {
                StoreName = StoreName,
                Date = Date,
                Subtotal = Subtotal,
                Tax = Tax,
                Total = Total.Value,
                Category = Category,
                Notes = Notes,
                ImagePath = ImagePath,
                RawText = RawText
            };

            var receiptId = await _receiptRepository.InsertAsync(receipt);
            receipt.Id = receiptId;

            // Save line items
            if (LineItems.Any())
            {
                var items = LineItems.Select((item, index) => new LineItem
                {
                    ReceiptId = receiptId,
                    Description = item.Description ?? "",
                    Quantity = item.Quantity,
                    UnitPrice = item.UnitPrice,
                    LineTotal = item.LineTotal,
                    Position = index
                }).ToList();

                await _lineItemRepository.BulkUpsertAsync(receiptId, items);
            }

            StatusMessage = "Receipt saved successfully!";

            // Navigate back to receipts list
            await Task.Delay(500);
            // Shell.Current.GoToAsync("//receipts");
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error saving: {ex.Message}";
        }
        finally
        {
            IsSaving = false;
        }
    }

    [RelayCommand]
    private void Cancel()
    {
        // Navigate back
        // Shell.Current.GoToAsync("..");
    }
}

public class LineItemEdit : ObservableObject
{
    private string? _description;
    private decimal? _quantity;
    private decimal? _unitPrice;
    private decimal? _lineTotal;

    public string? Description
    {
        get => _description;
        set => SetProperty(ref _description, value);
    }

    public decimal? Quantity
    {
        get => _quantity;
        set => SetProperty(ref _quantity, value);
    }

    public decimal? UnitPrice
    {
        get => _unitPrice;
        set => SetProperty(ref _unitPrice, value);
    }

    public decimal? LineTotal
    {
        get => _lineTotal;
        set => SetProperty(ref _lineTotal, value);
    }
}
