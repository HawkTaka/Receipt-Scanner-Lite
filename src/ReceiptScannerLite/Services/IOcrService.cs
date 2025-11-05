namespace ReceiptScannerLite.Services;

public interface IOcrService
{
    /// <summary>
    /// Performs OCR on a preprocessed image.
    /// </summary>
    /// <param name="imagePathPng">Path to the preprocessed PNG image</param>
    /// <param name="cancellationToken">Optional cancellation token to cancel the operation</param>
    /// <returns>Recognized text</returns>
    Task<string> RecognizeAsync(string imagePathPng, CancellationToken cancellationToken = default);
}
