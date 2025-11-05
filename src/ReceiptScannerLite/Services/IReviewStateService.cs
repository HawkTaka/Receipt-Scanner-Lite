namespace ReceiptScannerLite.Services;

/// <summary>
/// Service for managing review page state during navigation.
/// Scoped per request/page lifecycle to avoid thread-safety issues.
/// </summary>
public interface IReviewStateService
{
    /// <summary>
    /// Sets the review state for the next navigation to the review page.
    /// </summary>
    void SetReviewState(string imagePath, string rawText, ParseResult? parseResult);

    /// <summary>
    /// Gets the current review state and clears it.
    /// </summary>
    ReviewState? GetAndClearReviewState();

    /// <summary>
    /// Checks if review state is available.
    /// </summary>
    bool HasReviewState { get; }
}

/// <summary>
/// Data container for review page state.
/// </summary>
public class ReviewState
{
    public string ImagePath { get; set; } = "";
    public string RawText { get; set; } = "";
    public ParseResult? ParseResult { get; set; }
}
