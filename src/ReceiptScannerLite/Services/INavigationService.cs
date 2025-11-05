namespace ReceiptScannerLite.Services;

public interface INavigationService
{
    /// <summary>
    /// Navigates to the specified URI.
    /// </summary>
    /// <param name="uri">The destination URI</param>
    void NavigateTo(string uri);

    /// <summary>
    /// Navigates to the review page with receipt data.
    /// </summary>
    /// <param name="imagePath">Path to the receipt image</param>
    /// <param name="rawText">Raw OCR text</param>
    /// <param name="parseResult">Parsed receipt data</param>
    void NavigateToReview(string imagePath, string rawText, ParseResult? parseResult);

    /// <summary>
    /// Navigates to the receipt detail page.
    /// </summary>
    /// <param name="receiptId">ID of the receipt to view</param>
    void NavigateToReceipt(int receiptId);

    /// <summary>
    /// Navigates to the edit receipt page.
    /// </summary>
    /// <param name="receiptId">ID of the receipt to edit</param>
    void NavigateToEditReceipt(int receiptId);

    /// <summary>
    /// Navigates back to the previous page.
    /// </summary>
    void NavigateBack();
}
