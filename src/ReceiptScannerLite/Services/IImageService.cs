namespace ReceiptScannerLite.Services;

public interface IImageService
{
    /// <summary>
    /// Converts an image file to a base64 data URL for display in Blazor.
    /// </summary>
    /// <param name="filePath">Path to the image file</param>
    /// <returns>Base64 data URL (e.g., "data:image/jpeg;base64,...")</returns>
    Task<string?> GetImageDataUrlAsync(string? filePath);

    /// <summary>
    /// Gets the image bytes from a file path.
    /// </summary>
    /// <param name="filePath">Path to the image file</param>
    /// <returns>Image bytes or null if file doesn't exist</returns>
    Task<byte[]?> GetImageBytesAsync(string? filePath);
}
