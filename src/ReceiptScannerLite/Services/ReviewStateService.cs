namespace ReceiptScannerLite.Services;

/// <summary>
/// Scoped service for managing review page state.
/// Each page/request gets its own instance, avoiding static state issues.
/// </summary>
public class ReviewStateService : IReviewStateService
{
    private ReviewState? _currentState;

    public bool HasReviewState => _currentState != null;

    public void SetReviewState(string imagePath, string rawText, ParseResult? parseResult)
    {
        _currentState = new ReviewState
        {
            ImagePath = imagePath,
            RawText = rawText,
            ParseResult = parseResult
        };
    }

    public ReviewState? GetAndClearReviewState()
    {
        var state = _currentState;
        _currentState = null; // Clear after reading
        return state;
    }
}
