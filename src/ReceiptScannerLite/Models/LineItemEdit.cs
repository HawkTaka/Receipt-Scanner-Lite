using CommunityToolkit.Mvvm.ComponentModel;

namespace ReceiptScannerLite.Models;

/// <summary>
/// Editable model for line items used in review and edit screens
/// </summary>
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
