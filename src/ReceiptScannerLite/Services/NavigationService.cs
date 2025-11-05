using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;

namespace ReceiptScannerLite.Services;

public class NavigationService : INavigationService
{
    private NavigationManager? _navigationManager;
    private readonly ILogger<NavigationService> _logger;

    public NavigationService(ILogger<NavigationService> logger)
    {
        _logger = logger;
    }

    public void Initialize(NavigationManager navigationManager)
    {
        _navigationManager = navigationManager;
        _logger.LogDebug("NavigationManager initialized");
    }

    public void NavigateTo(string uri)
    {
        if (_navigationManager == null)
        {
            _logger.LogWarning("NavigationManager not initialized. Cannot navigate to {Uri}", uri);
            return;
        }

        _navigationManager.NavigateTo(uri);
        _logger.LogDebug("Navigated to {Uri}", uri);
    }

    public void NavigateToReview(string imagePath, string rawText, ParseResult? parseResult)
    {
        // Store the data in a shared state service for the review page to pick up
        var reviewState = new ReviewState
        {
            ImagePath = imagePath,
            RawText = rawText,
            ParseResult = parseResult
        };

        // In a real app, you'd store this in a state service
        // For now, we'll use a simple static holder (not ideal but functional)
        ReviewStateHolder.Current = reviewState;

        NavigateTo("/review");
    }

    public void NavigateToReceipt(int receiptId)
    {
        NavigateTo($"/receipt-detail/{receiptId}");
    }

    public void NavigateToEditReceipt(int receiptId)
    {
        NavigateTo($"/edit-receipt/{receiptId}");
    }

    public void NavigateBack()
    {
        if (_navigationManager == null)
        {
            _logger.LogWarning("NavigationManager not initialized. Cannot navigate back");
            return;
        }

        // Blazor doesn't have a built-in NavigateBack
        // Navigate to receipts as default fallback
        NavigateTo("/receipts");
    }
}

/// <summary>
/// Temporary state holder for review page data.
/// In production, use a proper state management solution.
/// </summary>
public static class ReviewStateHolder
{
    public static ReviewState? Current { get; set; }
}

public class ReviewState
{
    public string ImagePath { get; set; } = "";
    public string RawText { get; set; } = "";
    public ParseResult? ParseResult { get; set; }
}
