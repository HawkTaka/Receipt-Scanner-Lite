namespace ReceiptScannerLite.Services;

public interface IFileService
{
    /// <summary>
    /// Captures a photo using the camera.
    /// </summary>
    /// <returns>Path to the captured image, or null if cancelled</returns>
    Task<string?> CapturePhotoAsync();

    /// <summary>
    /// Picks a photo from the gallery.
    /// </summary>
    /// <returns>Path to the selected image, or null if cancelled</returns>
    Task<string?> PickPhotoAsync();

    /// <summary>
    /// Saves an image to the app's data directory.
    /// </summary>
    /// <param name="sourcePath">Source image path</param>
    /// <returns>Path to the saved image in app data</returns>
    Task<string> SaveImageAsync(string sourcePath);

    /// <summary>
    /// Deletes a file if it exists.
    /// </summary>
    /// <param name="filePath">Path to the file</param>
    Task DeleteFileAsync(string filePath);
}
