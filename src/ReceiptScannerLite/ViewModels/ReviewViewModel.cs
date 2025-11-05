using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ReceiptScannerLite.Data.Models;
using ReceiptScannerLite.Data.Repositories;
using ReceiptScannerLite.Services;
using System.Collections.ObjectModel;

namespace ReceiptScannerLite.ViewModels;

public partial class ReviewViewModel : ObservableObject
{
    private readonly IReceiptRepository _receiptRepository;
    private readonly ILineItemRepository _lineItemRepository;
    private readonly INavigationService _navigationService;
    private readonly IReceiptValidationService _validationService;
    private readonly ICategoryService _categoryService;
    private readonly IErrorMessageService _errorMessageService;

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
    private string _category;

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

    [ObservableProperty]
    private string? _totalError;

    [ObservableProperty]
    private string? _dateError;

    [ObservableProperty]
    private string? _categoryError;

    [ObservableProperty]
    private bool _hasValidationErrors;

    public ObservableCollection<LineItemEdit> LineItems { get; } = new();

    public IReadOnlyList<string> Categories { get; }

    public ReviewViewModel(
        IReceiptRepository receiptRepository,
        ILineItemRepository lineItemRepository,
        INavigationService navigationService,
        IReceiptValidationService validationService,
        ICategoryService categoryService,
        IErrorMessageService errorMessageService)
    {
        _receiptRepository = receiptRepository;
        _lineItemRepository = lineItemRepository;
        _navigationService = navigationService;
        _validationService = validationService;
        _categoryService = categoryService;
        _errorMessageService = errorMessageService;

        Categories = _categoryService.GetAllCategories();
        _category = _categoryService.GetDefaultCategory();
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
        if (item == null)
            return;

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
        StatusMessage = "Validating receipt...";

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

            // Validate receipt before saving
            var validationResult = _validationService.Validate(receipt);
            if (!validationResult.IsValid)
            {
                StatusMessage = $"Validation failed:\n{string.Join("\n", validationResult.Errors)}";
                return;
            }

            StatusMessage = "Saving receipt...";
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

            // Navigate back to receipts list immediately
            // The successful navigation itself confirms the save succeeded
            _navigationService.NavigateTo("/receipts");
        }
        catch (Exception ex)
        {
            StatusMessage = _errorMessageService.GetSaveErrorMessage(ex);
        }
        finally
        {
            IsSaving = false;
        }
    }

    [RelayCommand]
    private void Cancel()
    {
        _navigationService.NavigateBack();
    }

    partial void OnTotalChanged(decimal? value)
    {
        ValidateTotal();
        UpdateValidationState();
    }

    partial void OnDateChanged(DateTime value)
    {
        ValidateDate();
        UpdateValidationState();
    }

    partial void OnCategoryChanged(string value)
    {
        ValidateCategory();
        UpdateValidationState();
    }

    partial void OnSubtotalChanged(decimal? value)
    {
        ValidateTotalCalculation();
        UpdateValidationState();
    }

    partial void OnTaxChanged(decimal? value)
    {
        ValidateTotalCalculation();
        UpdateValidationState();
    }

    private void ValidateTotal()
    {
        if (!Total.HasValue || Total.Value <= 0)
        {
            TotalError = "Total must be greater than zero";
        }
        else if (Total.Value > 1000000)
        {
            TotalError = "Total seems unusually high. Please verify.";
        }
        else
        {
            TotalError = null;
        }
    }

    private void ValidateDate()
    {
        if (Date > DateTime.Today)
        {
            DateError = "Receipt date cannot be in the future";
        }
        else if (Date < DateTime.Today.AddYears(-10))
        {
            DateError = "Receipt date is more than 10 years old";
        }
        else
        {
            DateError = null;
        }
    }

    private void ValidateCategory()
    {
        if (string.IsNullOrWhiteSpace(Category))
        {
            CategoryError = "Please select a category";
        }
        else if (!_categoryService.IsValidCategory(Category))
        {
            CategoryError = $"Invalid category. Please select from the list.";
        }
        else
        {
            CategoryError = null;
        }
    }

    private void ValidateTotalCalculation()
    {
        if (Subtotal.HasValue && Tax.HasValue && Total.HasValue)
        {
            var expectedTotal = Subtotal.Value + Tax.Value;
            var difference = Math.Abs(expectedTotal - Total.Value);
            const decimal tolerance = 0.02m;

            if (difference > tolerance)
            {
                TotalError = $"Total ({Total.Value:C}) doesn't match Subtotal + Tax ({expectedTotal:C})";
            }
            else if (!string.IsNullOrEmpty(TotalError) && TotalError.Contains("doesn't match"))
            {
                // Clear this specific error if it was set
                TotalError = null;
            }
        }
    }

    private void UpdateValidationState()
    {
        HasValidationErrors = !string.IsNullOrEmpty(TotalError) ||
                               !string.IsNullOrEmpty(DateError) ||
                               !string.IsNullOrEmpty(CategoryError);
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
