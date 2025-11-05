namespace ReceiptScannerLite.Services;

public interface IReceiptValidationService
{
    /// <summary>
    /// Validates a receipt and returns validation result with any errors.
    /// </summary>
    /// <param name="receipt">The receipt to validate</param>
    /// <returns>Validation result indicating if valid and any error messages</returns>
    ValidationResult Validate(Data.Models.Receipt receipt);
}

public class ValidationResult
{
    public bool IsValid { get; set; }
    public List<string> Errors { get; set; } = new();

    public static ValidationResult Success() => new ValidationResult { IsValid = true };

    public static ValidationResult Failure(params string[] errors)
    {
        return new ValidationResult
        {
            IsValid = false,
            Errors = errors.ToList()
        };
    }

    public void AddError(string error)
    {
        IsValid = false;
        Errors.Add(error);
    }
}
