using Microsoft.Extensions.Logging;
using ReceiptScannerLite.Data;

namespace ReceiptScannerLite.Services;

public class BootstrapService : IBootstrapService
{
    private readonly AppDb _db;
    private readonly ILogger<BootstrapService> _logger;

    public BootstrapService(AppDb db, ILogger<BootstrapService> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task InitializeAsync()
    {
        try
        {
            // Create app directories
            var receiptsDir = Path.Combine(FileSystem.Current.AppDataDirectory, Constants.Directories.Receipts);
            var tessdataDir = Path.Combine(FileSystem.Current.AppDataDirectory, "tessdata");
            var tempDir = Path.Combine(FileSystem.Current.CacheDirectory);

            Directory.CreateDirectory(receiptsDir);
            Directory.CreateDirectory(tessdataDir);
            Directory.CreateDirectory(tempDir);

            // Copy tessdata if needed
            await CopyTessdataAsync(tessdataDir);

            // Initialize database
            await _db.InitializeAsync();

            _logger.LogInformation("Bootstrap completed successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Bootstrap initialization failed");
            throw;
        }
    }

    private async Task CopyTessdataAsync(string targetDir)
    {
        var targetFile = Path.Combine(targetDir, "eng.traineddata");

        // Only copy if not already present
        if (File.Exists(targetFile))
        {
            _logger.LogDebug("Tessdata already exists at {TargetFile}, skipping copy", targetFile);
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
            _logger.LogInformation("Copied tessdata to {TargetFile}", targetFile);
#else
            // For Windows and other platforms
            var sourceFile = Path.Combine(AppContext.BaseDirectory, "Assets", "tessdata", "eng.traineddata");
            if (File.Exists(sourceFile))
            {
                File.Copy(sourceFile, targetFile);
                _logger.LogInformation("Copied tessdata to {TargetFile}", targetFile);
            }
            else
            {
                _logger.LogWarning("eng.traineddata not found at {SourceFile}. OCR functionality will not be available until tessdata is manually provided", sourceFile);
            }
#endif
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to copy tessdata. OCR functionality may not be available");
        }
    }
}
