using ReceiptScannerLite.Data.Models;

namespace ReceiptScannerLite.Services;

public class ReceiptValidationService : IReceiptValidationService
{
    private const decimal TotalTolerance = 0.02m; // 2 cent tolerance for rounding differences
    private readonly ICategoryService _categoryService;

    public ReceiptValidationService(ICategoryService categoryService)
    {
        _categoryService = categoryService;
    }

    public ValidationResult Validate(Receipt receipt)
    {
        var result = new ValidationResult { IsValid = true };

        // Validate Total
        if (receipt.Total <= 0)
        {
            result.AddError("Total must be greater than zero.");
        }

        // Validate Date - should not be in the future
        if (receipt.Date.Date > DateTime.Now.Date)
        {
            result.AddError("Receipt date cannot be in the future.");
        }

        // Validate Date - should not be too old (more than 10 years)
        if (receipt.Date < DateTime.Now.AddYears(-10))
        {
            result.AddError("Receipt date seems unusually old (more than 10 years ago).");
        }

        // Validate Subtotal if provided
        if (receipt.Subtotal.HasValue)
        {
            if (receipt.Subtotal.Value < 0)
            {
                result.AddError("Subtotal cannot be negative.");
            }

            if (receipt.Subtotal.Value > receipt.Total)
            {
                result.AddError("Subtotal cannot be greater than Total.");
            }
        }

        // Validate Tax if provided
        if (receipt.Tax.HasValue)
        {
            if (receipt.Tax.Value < 0)
            {
                result.AddError("Tax cannot be negative.");
            }

            if (receipt.Tax.Value > receipt.Total)
            {
                result.AddError("Tax cannot be greater than Total.");
            }
        }

        // Validate Subtotal + Tax = Total (with tolerance)
        if (receipt.Subtotal.HasValue && receipt.Tax.HasValue)
        {
            var expectedTotal = receipt.Subtotal.Value + receipt.Tax.Value;
            var difference = Math.Abs(expectedTotal - receipt.Total);

            if (difference > TotalTolerance)
            {
                result.AddError($"Total ({receipt.Total:C}) does not match Subtotal ({receipt.Subtotal.Value:C}) + Tax ({receipt.Tax.Value:C}) = {expectedTotal:C}");
            }
        }

        // Validate Category
        if (!_categoryService.IsValidCategory(receipt.Category))
        {
            result.AddError($"Invalid category '{receipt.Category}'. Must be one of: {string.Join(", ", _categoryService.GetAllCategories())}");
        }

        // Validate ImagePath exists and is within allowed directory
        if (!string.IsNullOrWhiteSpace(receipt.ImagePath))
        {
            // Security: Validate path is within the receipts directory (prevent path traversal)
            var receiptsDir = Path.Combine(FileSystem.Current.AppDataDirectory, "receipts");
            var fullImagePath = Path.GetFullPath(receipt.ImagePath);
            var fullReceiptsDir = Path.GetFullPath(receiptsDir);

            if (!fullImagePath.StartsWith(fullReceiptsDir, StringComparison.OrdinalIgnoreCase))
            {
                result.AddError($"Image path must be within the receipts directory. Path traversal is not allowed.");
            }
            else if (!File.Exists(receipt.ImagePath))
            {
                result.AddError($"Image file does not exist at path: {receipt.ImagePath}");
            }
        }

        // Validate StoreName length if provided
        if (!string.IsNullOrWhiteSpace(receipt.StoreName) && receipt.StoreName.Length > 200)
        {
            result.AddError("Store name is too long (maximum 200 characters).");
        }

        // Validate Notes length if provided
        if (!string.IsNullOrWhiteSpace(receipt.Notes) && receipt.Notes.Length > 1000)
        {
            result.AddError("Notes are too long (maximum 1000 characters).");
        }

        return result;
    }

    public ValidationResult ValidateWithLineItems(Receipt receipt, IEnumerable<LineItem> lineItems)
    {
        // First validate the receipt itself
        var result = Validate(receipt);

        // Validate line items
        var lineItemsList = lineItems.ToList();

        for (int i = 0; i < lineItemsList.Count; i++)
        {
            var item = lineItemsList[i];
            var itemNumber = i + 1;

            // Validate Description
            if (string.IsNullOrWhiteSpace(item.Description))
            {
                result.AddError($"Line item {itemNumber}: Description is required.");
            }
            else if (item.Description.Length > 200)
            {
                result.AddError($"Line item {itemNumber}: Description is too long (maximum 200 characters).");
            }

            // Validate Quantity
            if (item.Quantity.HasValue)
            {
                if (item.Quantity.Value <= 0)
                {
                    result.AddError($"Line item {itemNumber}: Quantity must be greater than zero.");
                }
                if (item.Quantity.Value > 10000)
                {
                    result.AddError($"Line item {itemNumber}: Quantity seems unusually high.");
                }
            }

            // Validate UnitPrice
            if (item.UnitPrice.HasValue)
            {
                if (item.UnitPrice.Value < 0)
                {
                    result.AddError($"Line item {itemNumber}: Unit price cannot be negative.");
                }
                if (item.UnitPrice.Value > 1000000)
                {
                    result.AddError($"Line item {itemNumber}: Unit price seems unusually high.");
                }
            }

            // Validate LineTotal
            if (item.LineTotal.HasValue)
            {
                if (item.LineTotal.Value < 0)
                {
                    result.AddError($"Line item {itemNumber}: Line total cannot be negative.");
                }
                if (item.LineTotal.Value > receipt.Total)
                {
                    result.AddError($"Line item {itemNumber}: Line total ({item.LineTotal.Value:C}) cannot exceed receipt total ({receipt.Total:C}).");
                }

                // Validate LineTotal = Quantity * UnitPrice (if both provided)
                if (item.Quantity.HasValue && item.UnitPrice.HasValue)
                {
                    var expectedTotal = item.Quantity.Value * item.UnitPrice.Value;
                    var difference = Math.Abs(expectedTotal - item.LineTotal.Value);

                    if (difference > TotalTolerance)
                    {
                        result.AddError($"Line item {itemNumber}: Line total ({item.LineTotal.Value:C}) does not match Quantity ({item.Quantity.Value}) × Unit Price ({item.UnitPrice.Value:C}) = {expectedTotal:C}");
                    }
                }
            }
        }

        // Validate sum of line totals doesn't exceed receipt total
        var lineItemsWithTotals = lineItemsList.Where(li => li.LineTotal.HasValue).ToList();
        if (lineItemsWithTotals.Any())
        {
            var sumOfLineItems = lineItemsWithTotals.Sum(li => li.LineTotal!.Value);
            if (sumOfLineItems > receipt.Total + TotalTolerance)
            {
                result.AddError($"Sum of line item totals ({sumOfLineItems:C}) exceeds receipt total ({receipt.Total:C}).");
            }
        }

        return result;
    }
}
