using Microsoft.Extensions.Logging;

namespace ReceiptScannerLite.Services;

public class FileService : IFileService
{
    private readonly string _receiptsDirectory;
    private readonly ILogger<FileService> _logger;

    public FileService(ILogger<FileService> logger)
    {
        _logger = logger;
        _receiptsDirectory = Path.Combine(FileSystem.Current.AppDataDirectory, Constants.Directories.Receipts);
        Directory.CreateDirectory(_receiptsDirectory);
        _logger.LogDebug("FileService initialized with receipts directory: {Directory}", _receiptsDirectory);
    }

    public async Task<string?> CapturePhotoAsync()
    {
        try
        {
            // Check camera permission first
            var cameraStatus = await Permissions.CheckStatusAsync<Permissions.Camera>();
            if (cameraStatus != PermissionStatus.Granted)
            {
                cameraStatus = await Permissions.RequestAsync<Permissions.Camera>();
            }

            if (cameraStatus != PermissionStatus.Granted)
            {
                throw new PermissionException("Camera permission is required to capture photos.");
            }

            var photo = await MediaPicker.Default.CapturePhotoAsync();
            if (photo == null)
                return null;

            return await SaveImageAsync(photo.FullPath);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error capturing photo");
            throw; // Re-throw to let caller handle permission denials
        }
    }

    public async Task<string?> PickPhotoAsync()
    {
        try
        {
            // Check photos permission first
            var photosStatus = await Permissions.CheckStatusAsync<Permissions.Photos>();
            if (photosStatus != PermissionStatus.Granted)
            {
                photosStatus = await Permissions.RequestAsync<Permissions.Photos>();
            }

            if (photosStatus != PermissionStatus.Granted)
            {
                throw new PermissionException("Photos access permission is required to select photos.");
            }

            var photo = await MediaPicker.Default.PickPhotoAsync();
            if (photo == null)
                return null;

            return await SaveImageAsync(photo.FullPath);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error picking photo");
            throw; // Re-throw to let caller handle permission denials
        }
    }

    public async Task<string> SaveImageAsync(string sourcePath)
    {
        var fileName = $"receipt_{Guid.NewGuid()}.jpg";
        var targetPath = Path.Combine(_receiptsDirectory, fileName);

        using var sourceStream = File.OpenRead(sourcePath);
        using var targetStream = File.Create(targetPath);
        await sourceStream.CopyToAsync(targetStream);

        return targetPath;
    }

    public Task DeleteFileAsync(string filePath)
    {
        try
        {
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error deleting file: {FilePath}", filePath);
        }

        return Task.CompletedTask;
    }
}
