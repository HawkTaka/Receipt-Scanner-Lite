namespace ReceiptScannerLite.Services;

public interface IOcrService
{
    /// <summary>
    /// Performs OCR on a preprocessed image.
    /// </summary>
    /// <param name="imagePathPng">Path to the preprocessed PNG image</param>
    /// <returns>Recognized text</returns>
    Task<string> RecognizeAsync(string imagePathPng);
}
