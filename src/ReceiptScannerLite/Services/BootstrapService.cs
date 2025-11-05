using ReceiptScannerLite.Data;

namespace ReceiptScannerLite.Services;

public class BootstrapService : IBootstrapService
{
    private readonly AppDb _db;

    public BootstrapService(AppDb db)
    {
        _db = db;
    }

    public async Task InitializeAsync()
    {
        try
        {
            // Create app directories
            var receiptsDir = Path.Combine(FileSystem.Current.AppDataDirectory, "receipts");
            var tessdataDir = Path.Combine(FileSystem.Current.AppDataDirectory, "tessdata");
            var tempDir = Path.Combine(FileSystem.Current.CacheDirectory);

            Directory.CreateDirectory(receiptsDir);
            Directory.CreateDirectory(tessdataDir);
            Directory.CreateDirectory(tempDir);

            // Copy tessdata if needed
            await CopyTessdataAsync(tessdataDir);

            // Initialize database
            await _db.InitializeAsync();

            Console.WriteLine("Bootstrap completed successfully");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Bootstrap error: {ex.Message}");
            throw;
        }
    }

    private async Task CopyTessdataAsync(string targetDir)
    {
        var targetFile = Path.Combine(targetDir, "eng.traineddata");

        // Only copy if not already present
        if (File.Exists(targetFile))
        {
            Console.WriteLine("Tessdata already exists, skipping copy");
            return;
        }

        try
        {
            // Try to read from the bundled assets
            // Note: The actual asset path varies by platform
            // For Android, it would be from Assets folder
            // For Windows, it would be from the app directory

#if ANDROID
            using var assetStream = await FileSystem.OpenAppPackageFileAsync("tessdata/eng.traineddata");
            using var targetStream = File.Create(targetFile);
            await assetStream.CopyToAsync(targetStream);
            Console.WriteLine($"Copied tessdata to: {targetFile}");
#else
            // For Windows and other platforms
            var sourceFile = Path.Combine(AppContext.BaseDirectory, "Assets", "tessdata", "eng.traineddata");
            if (File.Exists(sourceFile))
            {
                File.Copy(sourceFile, targetFile);
                Console.WriteLine($"Copied tessdata to: {targetFile}");
            }
            else
            {
                Console.WriteLine($"Warning: eng.traineddata not found at {sourceFile}");
                Console.WriteLine("OCR functionality will not be available until tessdata is manually provided.");
            }
#endif
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Warning: Failed to copy tessdata: {ex.Message}");
            Console.WriteLine("OCR functionality may not be available.");
        }
    }
}
