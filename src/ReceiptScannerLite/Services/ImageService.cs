namespace ReceiptScannerLite.Services;

public class ImageService : IImageService
{
    public async Task<string?> GetImageDataUrlAsync(string? filePath)
    {
        if (string.IsNullOrWhiteSpace(filePath) || !File.Exists(filePath))
        {
            return null;
        }

        try
        {
            var bytes = await File.ReadAllBytesAsync(filePath);
            var base64 = Convert.ToBase64String(bytes);

            // Determine MIME type from file extension
            var mimeType = GetMimeType(filePath);

            return $"data:{mimeType};base64,{base64}";
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error converting image to data URL: {ex.Message}");
            return null;
        }
    }

    public async Task<byte[]?> GetImageBytesAsync(string? filePath)
    {
        if (string.IsNullOrWhiteSpace(filePath) || !File.Exists(filePath))
        {
            return null;
        }

        try
        {
            return await File.ReadAllBytesAsync(filePath);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error reading image bytes: {ex.Message}");
            return null;
        }
    }

    private string GetMimeType(string filePath)
    {
        var extension = Path.GetExtension(filePath).ToLowerInvariant();

        return extension switch
        {
            ".jpg" or ".jpeg" => "image/jpeg",
            ".png" => "image/png",
            ".gif" => "image/gif",
            ".bmp" => "image/bmp",
            ".webp" => "image/webp",
            _ => "image/jpeg" // Default to JPEG
        };
    }
}
