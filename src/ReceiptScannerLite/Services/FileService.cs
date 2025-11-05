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
            var photo = await MediaPicker.Default.CapturePhotoAsync();
            if (photo == null)
                return null;

            return await SaveImageAsync(photo.FullPath);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error capturing photo: {ex.Message}");
            return null;
        }
    }

    public async Task<string?> PickPhotoAsync()
    {
        try
        {
            var photo = await MediaPicker.Default.PickPhotoAsync();
            if (photo == null)
                return null;

            return await SaveImageAsync(photo.FullPath);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error picking photo: {ex.Message}");
            return null;
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
