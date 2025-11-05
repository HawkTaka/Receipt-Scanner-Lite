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
    private readonly INavigationService _navigationService;
    private readonly IErrorMessageService _errorMessageService;

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
        IParseService parseService,
        INavigationService navigationService,
        IErrorMessageService errorMessageService)
    {
        _fileService = fileService;
        _preprocessService = preprocessService;
        _ocrService = ocrService;
        _parseService = parseService;
        _navigationService = navigationService;
        _errorMessageService = errorMessageService;
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
        catch (PermissionException ex)
        {
            StatusMessage = _errorMessageService.GetFriendlyMessage(ex, "capturing photo");
        }
        catch (Exception ex)
        {
            StatusMessage = _errorMessageService.GetImageErrorMessage(ex);
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
        catch (PermissionException ex)
        {
            StatusMessage = _errorMessageService.GetFriendlyMessage(ex, "selecting photo");
        }
        catch (Exception ex)
        {
            StatusMessage = _errorMessageService.GetImageErrorMessage(ex);
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

        string? preprocessedPath = null;

        try
        {
            // Preprocess
            StatusMessage = "Preprocessing image...";
            preprocessedPath = await _preprocessService.PrepareForOcrAsync(CapturedImagePath);

            // OCR
            StatusMessage = "Running OCR...";
            var rawText = await _ocrService.RecognizeAsync(preprocessedPath);

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
            StatusMessage = _errorMessageService.GetOcrErrorMessage(ex);
        }
        finally
        {
            // Cleanup temp file - ensure this happens even on error
            if (preprocessedPath != null)
            {
                await _fileService.DeleteFileAsync(preprocessedPath);
            }

            IsProcessing = false;
        }
    }

    private Task NavigateToReview(string rawText, ParseResult? parseResult)
    {
        if (string.IsNullOrEmpty(CapturedImagePath))
        {
            StatusMessage = "Error: No image captured";
            return Task.CompletedTask;
        }

        _navigationService.NavigateToReview(CapturedImagePath, rawText, parseResult);
        return Task.CompletedTask;
    }

    [RelayCommand]
    private void ClearImage()
    {
        CapturedImagePath = null;
        StatusMessage = null;
    }
}
