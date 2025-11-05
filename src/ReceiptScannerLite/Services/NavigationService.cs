using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;

namespace ReceiptScannerLite.Services;

public class NavigationService : INavigationService
{
    private NavigationManager? _navigationManager;
    private readonly ILogger<NavigationService> _logger;
    private readonly IReviewStateService _reviewStateService;

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

        // Blazor doesn't have a built-in NavigateBack
        // Navigate to receipts as default fallback
        NavigateTo("/receipts");
    }
}
