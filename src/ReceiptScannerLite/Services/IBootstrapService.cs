namespace ReceiptScannerLite.Services;

public interface IBootstrapService
{
    /// <summary>
    /// Initializes the application on first run.
    /// Copies tessdata, creates directories, and initializes the database.
    /// </summary>
    Task InitializeAsync();
}
