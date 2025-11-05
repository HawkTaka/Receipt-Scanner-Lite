using Microsoft.Extensions.Logging;
using ReceiptScannerLite.Data;
using ReceiptScannerLite.Data.Repositories;
using ReceiptScannerLite.Services;
using ReceiptScannerLite.ViewModels;

namespace ReceiptScannerLite;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
            });

        builder.Services.AddMauiBlazorWebView();

#if DEBUG
        builder.Services.AddBlazorWebViewDeveloperTools();
        builder.Logging.AddDebug();
#endif

        // Database
        var dbPath = Path.Combine(FileSystem.AppDataDirectory, "receipts.db");
        builder.Services.AddSingleton(sp => new AppDb(dbPath));

        // Repositories
        builder.Services.AddSingleton<IReceiptRepository, ReceiptRepository>();
        builder.Services.AddSingleton<ILineItemRepository, LineItemRepository>();

        // Services
        builder.Services.AddSingleton<IBootstrapService, BootstrapService>();
        builder.Services.AddSingleton<IFileService, FileService>();
        builder.Services.AddSingleton<IImagePreprocessService, ImagePreprocessService>();
        builder.Services.AddSingleton<IParseService, ParseService>();
        builder.Services.AddSingleton<ICsvExportService, CsvExportService>();

        // OCR Service - requires tessdata path
        builder.Services.AddSingleton<IOcrService>(sp =>
        {
            var tessdataPath = Path.Combine(FileSystem.AppDataDirectory, "tessdata");
            return new OcrService(tessdataPath);
        });

        // ViewModels
        builder.Services.AddTransient<CaptureViewModel>();
        builder.Services.AddTransient<ReviewViewModel>();
        builder.Services.AddTransient<ReceiptsViewModel>();
        builder.Services.AddTransient<ReceiptDetailViewModel>();
        builder.Services.AddTransient<InsightsViewModel>();
        builder.Services.AddTransient<ExportViewModel>();

        var app = builder.Build();

        // Initialize app on startup
        Task.Run(async () =>
        {
            try
            {
                var bootstrap = app.Services.GetRequiredService<IBootstrapService>();
                await bootstrap.InitializeAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Bootstrap error: {ex.Message}");
            }
        });

        return app;
    }
}
