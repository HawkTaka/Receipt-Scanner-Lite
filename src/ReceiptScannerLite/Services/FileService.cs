namespace ReceiptScannerLite.Services;

public class FileService : IFileService
{
    private readonly string _receiptsDirectory;

    public FileService()
    {
        _receiptsDirectory = Path.Combine(FileSystem.Current.AppDataDirectory, "receipts");
        Directory.CreateDirectory(_receiptsDirectory);
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
            Console.WriteLine($"Error capturing photo: {ex.Message}");
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
            Console.WriteLine($"Error picking photo: {ex.Message}");
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
            Console.WriteLine($"Error deleting file: {ex.Message}");
        }

        return Task.CompletedTask;
    }
}
