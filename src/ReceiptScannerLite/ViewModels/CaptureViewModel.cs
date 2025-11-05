using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ReceiptScannerLite.Services;

namespace ReceiptScannerLite.ViewModels;

public partial class CaptureViewModel : ObservableObject
{
    private readonly IFileService _fileService;
    private readonly IImagePreprocessService _preprocessService;
    private readonly IOcrService _ocrService;
    private readonly IParseService _parseService;

    [ObservableProperty]
    private string? _capturedImagePath;

    [ObservableProperty]
    private bool _isProcessing;

    [ObservableProperty]
    private string? _statusMessage;

    public CaptureViewModel(
        IFileService fileService,
        IImagePreprocessService preprocessService,
        IOcrService ocrService,
        IParseService parseService)
    {
        _fileService = fileService;
        _preprocessService = preprocessService;
        _ocrService = ocrService;
        _parseService = parseService;
    }

    [RelayCommand]
    private async Task CapturePhotoAsync()
    {
        try
        {
            StatusMessage = "Opening camera...";
            var path = await _fileService.CapturePhotoAsync();

            if (path != null)
            {
                CapturedImagePath = path;
                StatusMessage = "Photo captured successfully!";
            }
            else
            {
                StatusMessage = "Capture cancelled.";
            }
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error: {ex.Message}";
        }
    }

    [RelayCommand]
    private async Task PickPhotoAsync()
    {
        try
        {
            StatusMessage = "Opening gallery...";
            var path = await _fileService.PickPhotoAsync();

            if (path != null)
            {
                CapturedImagePath = path;
                StatusMessage = "Photo selected successfully!";
            }
            else
            {
                StatusMessage = "Selection cancelled.";
            }
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error: {ex.Message}";
        }
    }

    [RelayCommand]
    private async Task ProcessImageAsync()
    {
        if (string.IsNullOrEmpty(CapturedImagePath))
        {
            StatusMessage = "No image to process.";
            return;
        }

        IsProcessing = true;
        StatusMessage = "Processing image...";

        try
        {
            // Preprocess
            StatusMessage = "Preprocessing image...";
            var preprocessedPath = await _preprocessService.PrepareForOcrAsync(CapturedImagePath);

            // OCR
            StatusMessage = "Running OCR...";
            var rawText = await _ocrService.RecognizeAsync(preprocessedPath);

            // Cleanup temp file
            await _fileService.DeleteFileAsync(preprocessedPath);

            if (string.IsNullOrWhiteSpace(rawText))
            {
                StatusMessage = "No text detected. You can enter details manually.";
                // Navigate to review with empty data
                await NavigateToReview("", null);
                return;
            }

            // Parse
            StatusMessage = "Parsing receipt data...";
            var parseResult = _parseService.Parse(rawText);

            StatusMessage = "OCR completed successfully!";

            // Navigate to review page
            await NavigateToReview(rawText, parseResult);
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error: {ex.Message}";
        }
        finally
        {
            IsProcessing = false;
        }
    }

    private Task NavigateToReview(string rawText, ParseResult? parseResult)
    {
        // This will be implemented with actual navigation
        // For now, just a placeholder
        // In a real app, you'd use Shell.Current.GoToAsync or pass data via a service
        StatusMessage = "Ready to review. (Navigation not yet implemented)";
        return Task.CompletedTask;
    }

    [RelayCommand]
    private void ClearImage()
    {
        CapturedImagePath = null;
        StatusMessage = null;
    }
}
