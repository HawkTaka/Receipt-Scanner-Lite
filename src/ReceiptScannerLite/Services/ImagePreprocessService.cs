using SkiaSharp;

namespace ReceiptScannerLite.Services;

public class ImagePreprocessService : IImagePreprocessService
{
    private const int MaxLongEdge = 2000;
    private const float ContrastMultiplier = 1.2f;

    public async Task<string> PrepareForOcrAsync(string inputImagePath)
    {
        return await Task.Run(() =>
        {
            try
            {
                // Load the image
                using var inputStream = File.OpenRead(inputImagePath);
                using var original = SKBitmap.Decode(inputStream);

                if (original == null)
                {
                    throw new InvalidOperationException($"Failed to decode image: {inputImagePath}");
                }

                // Step 1: Scale down to max long edge
                var scaled = ScaleImage(original, MaxLongEdge);

                // Step 2: Convert to grayscale
                var grayscale = ConvertToGrayscale(scaled);

                // Step 3: Apply contrast boost
                var contrasted = ApplyContrast(grayscale, ContrastMultiplier);

                // Step 4: Apply adaptive threshold (binarize)
                var binarized = ApplyAdaptiveThreshold(contrasted);

                // Step 5: Save to temp PNG
                var tempPath = Path.Combine(
                    FileSystem.Current.CacheDirectory,
                    $"ocr_temp_{Guid.NewGuid()}.png");

                using var outputStream = File.OpenWrite(tempPath);
                binarized.Encode(outputStream, SKEncodedImageFormat.Png, 100);

                // Cleanup intermediate bitmaps
                if (scaled != original)
                    scaled.Dispose();
                grayscale.Dispose();
                contrasted.Dispose();
                binarized.Dispose();

                return tempPath;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Image preprocessing failed: {ex.Message}", ex);
            }
        });
    }

    private SKBitmap ScaleImage(SKBitmap original, int maxLongEdge)
    {
        var width = original.Width;
        var height = original.Height;
        var longEdge = Math.Max(width, height);

        if (longEdge <= maxLongEdge)
        {
            return original; // No scaling needed
        }

        var scale = (float)maxLongEdge / longEdge;
        var newWidth = (int)(width * scale);
        var newHeight = (int)(height * scale);

        var scaled = original.Resize(new SKImageInfo(newWidth, newHeight), SKFilterQuality.High);
        if (scaled == null)
        {
            throw new InvalidOperationException("Failed to scale image");
        }

        return scaled;
    }

    private SKBitmap ConvertToGrayscale(SKBitmap source)
    {
        var info = new SKImageInfo(source.Width, source.Height, SKColorType.Gray8);
        var grayscale = new SKBitmap(info);

        using var canvas = new SKCanvas(grayscale);
        using var paint = new SKPaint
        {
            ColorFilter = SKColorFilter.CreateColorMatrix(new float[]
            {
                0.21f, 0.72f, 0.07f, 0, 0,  // Red channel -> Gray
                0.21f, 0.72f, 0.07f, 0, 0,  // Green channel -> Gray
                0.21f, 0.72f, 0.07f, 0, 0,  // Blue channel -> Gray
                0,     0,     0,     1, 0   // Alpha channel
            })
        };

        canvas.DrawBitmap(source, 0, 0, paint);
        return grayscale;
    }

    private SKBitmap ApplyContrast(SKBitmap source, float contrast)
    {
        var result = new SKBitmap(source.Width, source.Height);
        var pixels = source.Pixels;

        for (int i = 0; i < pixels.Length; i++)
        {
            var pixel = pixels[i];
            var gray = pixel.Red; // Gray8 has same value in all channels

            // Apply contrast: newValue = (oldValue - 128) * contrast + 128
            var adjusted = (int)((gray - 128) * contrast + 128);
            adjusted = Math.Clamp(adjusted, 0, 255);

            pixels[i] = new SKColor((byte)adjusted, (byte)adjusted, (byte)adjusted, pixel.Alpha);
        }

        result.Pixels = pixels;
        return result;
    }

    private SKBitmap ApplyAdaptiveThreshold(SKBitmap source)
    {
        // Simple adaptive thresholding using local mean
        const int blockSize = 15; // Must be odd
        const int c = 5; // Constant subtracted from mean

        var result = new SKBitmap(source.Width, source.Height);
        var pixels = source.Pixels;
        var width = source.Width;
        var height = source.Height;

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                // Calculate local mean
                int sum = 0;
                int count = 0;
                int halfBlock = blockSize / 2;

                for (int by = Math.Max(0, y - halfBlock); by <= Math.Min(height - 1, y + halfBlock); by++)
                {
                    for (int bx = Math.Max(0, x - halfBlock); bx <= Math.Min(width - 1, x + halfBlock); bx++)
                    {
                        sum += pixels[by * width + bx].Red;
                        count++;
                    }
                }

                var mean = sum / count;
                var threshold = mean - c;
                var pixelValue = pixels[y * width + x].Red;

                // Binarize: if pixel > threshold, white; else black
                var binaryValue = pixelValue > threshold ? (byte)255 : (byte)0;
                pixels[y * width + x] = new SKColor(binaryValue, binaryValue, binaryValue, 255);
            }
        }

        result.Pixels = pixels;
        return result;
    }
}
