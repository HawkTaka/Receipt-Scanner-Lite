using ReceiptScannerLite.Data.Models;

namespace ReceiptScannerLite.Services;

public class ReceiptValidationService : IReceiptValidationService
{
    private const decimal TotalTolerance = 0.02m; // 2 cent tolerance for rounding differences

    private static readonly HashSet<string> ValidCategories = new()
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
        if (!ValidCategories.Contains(receipt.Category))
        {
            result.AddError($"Invalid category '{receipt.Category}'. Must be one of: {string.Join(", ", ValidCategories)}");
        }

        // Validate ImagePath exists if provided
        if (!string.IsNullOrWhiteSpace(receipt.ImagePath) && !File.Exists(receipt.ImagePath))
        {
            result.AddError($"Image file does not exist at path: {receipt.ImagePath}");
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
}
