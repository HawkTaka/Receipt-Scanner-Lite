using Microsoft.Extensions.Logging;
using ReceiptScannerLite.Data;
using ReceiptScannerLite.Data.Migrations;
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
        builder.Services.AddSingleton(sp =>
        {
            var migrationLogger = sp.GetRequiredService<ILogger<DatabaseMigrationService>>();
            return new AppDb(dbPath, migrationLogger);
        });

        // Repositories
        builder.Services.AddSingleton<IReceiptRepository, ReceiptRepository>();
        builder.Services.AddSingleton<ILineItemRepository, LineItemRepository>();

        // Services
        builder.Services.AddSingleton<IBootstrapService, BootstrapService>();
        builder.Services.AddSingleton<IFileService, FileService>();
        builder.Services.AddSingleton<IImageService, ImageService>();
        builder.Services.AddSingleton<IImagePreprocessService, ImagePreprocessService>();
        builder.Services.AddSingleton<IParseService, ParseService>();
        builder.Services.AddSingleton<ICsvExportService, CsvExportService>();
        builder.Services.AddSingleton<INavigationService, NavigationService>();
        builder.Services.AddSingleton<ICategoryService, CategoryService>();
        builder.Services.AddSingleton<IReceiptValidationService, ReceiptValidationService>();
        builder.Services.AddSingleton<IDialogService, DialogService>();
        builder.Services.AddSingleton<IErrorMessageService, ErrorMessageService>();

        // Scoped services (per page/request lifecycle)
        builder.Services.AddScoped<IReviewStateService, ReviewStateService>();

        // OCR Service - requires tessdata path
        builder.Services.AddSingleton<IOcrService>(sp =>
        {
            var tessdataPath = Path.Combine(FileSystem.AppDataDirectory, "tessdata");
            var logger = sp.GetRequiredService<ILogger<OcrService>>();
            return new OcrService(tessdataPath, logger);
        });

        // ViewModels
        builder.Services.AddTransient<CaptureViewModel>();
        builder.Services.AddTransient<ReviewViewModel>();
        builder.Services.AddTransient<EditReceiptViewModel>();
        builder.Services.AddTransient<ReceiptsViewModel>();
        builder.Services.AddTransient<ReceiptDetailViewModel>();
        builder.Services.AddTransient<InsightsViewModel>();
        builder.Services.AddTransient<ExportViewModel>();

        var app = builder.Build();

        // Initialize app on startup - synchronously initialize critical components
        // This runs before UI is shown, so blocking is acceptable for essential setup
        try
        {
            var bootstrap = app.Services.GetRequiredService<IBootstrapService>();

            // Use GetAwaiter().GetResult() instead of Wait() for better async handling
            // Timeout after 10 seconds (reduced from 30s)
            var initTask = Task.Run(async () =>
            {
                var timeout = Task.Delay(TimeSpan.FromSeconds(10));
                var completed = await Task.WhenAny(bootstrap.InitializeAsync(), timeout);

                if (completed == timeout)
                {
                    var logger = app.Services.GetRequiredService<ILogger<MauiApp>>();
                    logger.LogWarning("Bootstrap initialization timed out after 10 seconds");
                    return false;
                }
                return true;
            });

            initTask.GetAwaiter().GetResult();
        }
        catch (Exception ex)
        {
            var logger = app.Services.GetRequiredService<ILogger<MauiApp>>();
            logger.LogError(ex, "Bootstrap initialization failed");
            // Continue anyway - app can still function with manual data entry
        }

        return app;
    }
}
