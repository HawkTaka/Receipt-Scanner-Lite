namespace ReceiptScannerLite.Services;

/// <summary>
/// Service for providing user-friendly error messages
/// </summary>
public interface IErrorMessageService
{
    /// <summary>
    /// Gets a user-friendly error message for an exception.
    /// </summary>
    /// <param name="exception">The exception that occurred</param>
    /// <param name="context">Context about what operation was being performed</param>
    /// <returns>A user-friendly error message</returns>
    string GetFriendlyMessage(Exception exception, string context);

    /// <summary>
    /// Gets a user-friendly error message for save operations.
    /// </summary>
    /// <param name="exception">The exception that occurred</param>
    /// <returns>A user-friendly error message</returns>
    string GetSaveErrorMessage(Exception exception);

    /// <summary>
    /// Gets a user-friendly error message for delete operations.
    /// </summary>
    /// <param name="exception">The exception that occurred</param>
    /// <returns>A user-friendly error message</returns>
    string GetDeleteErrorMessage(Exception exception);

    /// <summary>
    /// Gets a user-friendly error message for load operations.
    /// </summary>
    /// <param name="exception">The exception that occurred</param>
    /// <returns>A user-friendly error message</returns>
    string GetLoadErrorMessage(Exception exception);

    /// <summary>
    /// Gets a user-friendly error message for image operations.
    /// </summary>
    /// <param name="exception">The exception that occurred</param>
    /// <returns>A user-friendly error message</returns>
    string GetImageErrorMessage(Exception exception);

    /// <summary>
    /// Gets a user-friendly error message for OCR operations.
    /// </summary>
    /// <param name="exception">The exception that occurred</param>
    /// <returns>A user-friendly error message</returns>
    string GetOcrErrorMessage(Exception exception);
}
