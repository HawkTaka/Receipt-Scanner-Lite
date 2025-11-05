using Tesseract;

namespace ReceiptScannerLite.Services;

public class OcrService : IOcrService, IDisposable
{
    private readonly TesseractEngine? _engine;
    private readonly SemaphoreSlim _engineLock = new SemaphoreSlim(1, 1);
    private bool _disposed;

    public OcrService(string tessdataPath)
    {
        try
        {
            _engine = new TesseractEngine(tessdataPath, "eng", EngineMode.Default);
        }
        catch (Exception ex)
        {
            // If Tesseract initialization fails, log but don't crash
            // This allows the app to function with manual entry
            Console.WriteLine($"Warning: Tesseract initialization failed: {ex.Message}");
            _engine = null;
        }
    }

    public async Task<string> RecognizeAsync(string imagePathPng)
    {
        if (_engine == null)
        {
            throw new InvalidOperationException("OCR engine is not initialized. Tesseract may not be properly configured.");
        }

        // Use semaphore to ensure only one thread accesses the engine at a time
        // TesseractEngine is not thread-safe
        await _engineLock.WaitAsync();
        try
        {
            return await Task.Run(() =>
            {
                try
                {
                    using var img = Pix.LoadFromFile(imagePathPng);
                    using var page = _engine.Process(img, PageSegMode.Auto);
                    var text = page.GetText();
                    return text ?? string.Empty;
                }
                catch (Exception ex)
                {
                    throw new InvalidOperationException($"OCR recognition failed: {ex.Message}", ex);
                }
            });
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
