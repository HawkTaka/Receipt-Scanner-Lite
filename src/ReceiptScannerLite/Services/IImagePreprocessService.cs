namespace ReceiptScannerLite.Services;

public interface IImagePreprocessService
{
    /// <summary>
    /// Preprocesses an image for OCR by applying scaling, grayscale conversion,
    /// contrast enhancement, and adaptive thresholding.
    /// </summary>
    /// <param name="inputImagePath">Path to the input image</param>
    /// <returns>Path to the temporary preprocessed PNG file</returns>
    Task<string> PrepareForOcrAsync(string inputImagePath);
}
