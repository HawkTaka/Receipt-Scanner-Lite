using Microsoft.Extensions.Logging;
using Tesseract;

namespace ReceiptScannerLite.Services;

public class OcrService : IOcrService, IDisposable
{
    private readonly TesseractEngine? _engine;
    private readonly SemaphoreSlim _engineLock = new SemaphoreSlim(1, 1);
    private readonly ILogger<OcrService> _logger;
    private bool _disposed;

    public OcrService(string tessdataPath, ILogger<OcrService> logger)
    {
        _logger = logger;

        try
        {
            _engine = new TesseractEngine(tessdataPath, "eng", EngineMode.Default);
            _logger.LogInformation("Tesseract engine initialized successfully with tessdata path: {TessdataPath}", tessdataPath);
        }
        catch (Exception ex)
        {
            // If Tesseract initialization fails, log but don't crash
            // This allows the app to function with manual entry
            _logger.LogWarning(ex, "Tesseract initialization failed. App will function with manual entry only.");
            _engine = null;
        }
    }

    public async Task<string> RecognizeAsync(string imagePathPng, CancellationToken cancellationToken = default)
    {
        if (_engine == null)
        {
            throw new InvalidOperationException("OCR engine is not initialized. Tesseract may not be properly configured.");
        }

        // Use semaphore to ensure only one thread accesses the engine at a time
        // TesseractEngine is not thread-safe
        // Pass cancellation token to WaitAsync so we can cancel while waiting for lock
        await _engineLock.WaitAsync(cancellationToken);
        try
        {
            return await Task.Run(() =>
            {
                try
                {
                    // Check for cancellation before starting OCR
                    cancellationToken.ThrowIfCancellationRequested();

                    using var img = Pix.LoadFromFile(imagePathPng);
                    using var page = _engine.Process(img, PageSegMode.Auto);

                    // Check for cancellation before getting text (OCR already complete at this point)
                    cancellationToken.ThrowIfCancellationRequested();

                    var text = page.GetText();
                    return text ?? string.Empty;
                }
                catch (Exception ex) when (ex is not OperationCanceledException)
                {
                    throw new InvalidOperationException($"OCR recognition failed: {ex.Message}", ex);
                }
            }, cancellationToken);
        }
        finally
        {
            _engineLock.Release();
        }
    }

    public void Dispose()
    {
        if (!_disposed)
        {
            _engine?.Dispose();
            _engineLock?.Dispose();
            _disposed = true;
        }
    }
}
