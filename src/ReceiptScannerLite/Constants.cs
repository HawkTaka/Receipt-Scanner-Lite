namespace ReceiptScannerLite;

/// <summary>
/// Application-wide constants for commonly used values
/// </summary>
public static class Constants
{
    /// <summary>
    /// Directory names
    /// </summary>
    public static class Directories
    {
        /// <summary>
        /// Directory name for storing receipt images
        /// </summary>
        public const string Receipts = "receipts";
    }

    /// <summary>
    /// Database-related constants
    /// </summary>
    public static class Database
    {
        /// <summary>
        /// SQLite database filename
        /// </summary>
        public const string FileName = "receipts.db3";

        /// <summary>
        /// Database connection flags
        /// </summary>
        public const SQLite.SQLiteOpenFlags Flags =
            SQLite.SQLiteOpenFlags.ReadWrite |
            SQLite.SQLiteOpenFlags.Create |
            SQLite.SQLiteOpenFlags.SharedCache;
    }

    /// <summary>
    /// UI-related constants
    /// </summary>
    public static class UI
    {
        /// <summary>
        /// Default category filter value showing all categories
        /// </summary>
        public const string AllCategories = "All";
    }

    /// <summary>
    /// File-related constants
    /// </summary>
    public static class Files
    {
        /// <summary>
        /// Prefix for temporary OCR processing files
        /// </summary>
        public const string OcrTempPrefix = "ocr_temp_";

        /// <summary>
        /// PNG file extension
        /// </summary>
        public const string PngExtension = ".png";

        /// <summary>
        /// JPEG file extension
        /// </summary>
        public const string JpegExtension = ".jpg";
    }

    /// <summary>
    /// CSV Export constants
    /// </summary>
    public static class Export
    {
        /// <summary>
        /// Default receipts CSV filename
        /// </summary>
        public const string ReceiptsFileName = "receipts.csv";

        /// <summary>
        /// Default line items CSV filename
        /// </summary>
        public const string LineItemsFileName = "line_items.csv";
    }

    /// <summary>
    /// Timing-related constants
    /// </summary>
    public static class Timing
    {
        /// <summary>
        /// Debounce delay for search input in milliseconds
        /// </summary>
        public const int SearchDebounceMilliseconds = 400;

        /// <summary>
        /// Cache expiration time for insights in minutes
        /// </summary>
        public const int InsightsCacheMinutes = 5;

        /// <summary>
        /// Page size for receipt pagination
        /// </summary>
        public const int ReceiptsPageSize = 50;

        /// <summary>
        /// Maximum navigation stack size
        /// </summary>
        public const int MaxNavigationStackSize = 10;
    }
}
