using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;

namespace ReceiptScannerLite.Services;

public class NavigationService : INavigationService
{
    private NavigationManager? _navigationManager;
    private readonly ILogger<NavigationService> _logger;
    private readonly IReviewStateService _reviewStateService;
    private readonly Stack<string> _navigationStack = new Stack<string>();
    private const int MaxStackSize = 10; // Prevent unbounded growth

    public NavigationService(ILogger<NavigationService> logger, IReviewStateService reviewStateService)
    {
        _logger = logger;
        _reviewStateService = reviewStateService;
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

        // Track current location before navigating (for back navigation)
        var currentUri = _navigationManager.Uri;
        if (!string.IsNullOrEmpty(currentUri) && currentUri != uri)
        {
            // Only push if not already at top of stack (avoid duplicates on refresh)
            if (_navigationStack.Count == 0 || _navigationStack.Peek() != currentUri)
            {
                _navigationStack.Push(currentUri);

                // Limit stack size to prevent memory issues
                if (_navigationStack.Count > MaxStackSize)
                {
                    // Remove oldest entry (at bottom of stack)
                    var temp = _navigationStack.ToArray();
                    _navigationStack.Clear();
                    for (int i = 0; i < MaxStackSize; i++)
                    {
                        _navigationStack.Push(temp[i]);
                    }
                }
            }
        }

        _navigationManager.NavigateTo(uri);
        _logger.LogDebug("Navigated to {Uri}", uri);
    }

    public void NavigateToReview(string imagePath, string rawText, ParseResult? parseResult)
    {
        // Store the data in the scoped state service for the review page to pick up
        _reviewStateService.SetReviewState(imagePath, rawText, parseResult);
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

        // Try to pop from navigation stack
        if (_navigationStack.Count > 0)
        {
            var previousUri = _navigationStack.Pop();
            _logger.LogDebug("Navigating back to {Uri}", previousUri);

            // Navigate without pushing to stack (to avoid circular navigation)
            _navigationManager.NavigateTo(previousUri);
        }
        else
        {
            // No history available, go to default home page
            _logger.LogDebug("No navigation history, navigating to default /receipts");
            _navigationManager.NavigateTo("/receipts");
        }
    }
}
