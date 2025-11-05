namespace ReceiptScannerLite.Services;

public interface IDialogService
{
    /// <summary>
    /// Displays a confirmation dialog with yes/no options.
    /// </summary>
    /// <param name="title">Dialog title</param>
    /// <param name="message">Dialog message</param>
    /// <param name="accept">Text for accept button (default: "Yes")</param>
    /// <param name="cancel">Text for cancel button (default: "No")</param>
    /// <returns>True if user confirmed, false otherwise</returns>
    Task<bool> ConfirmAsync(string title, string message, string accept = "Yes", string cancel = "No");

    /// <summary>
    /// Displays an alert dialog with a single OK button.
    /// </summary>
    /// <param name="title">Dialog title</param>
    /// <param name="message">Dialog message</param>
    /// <param name="okButton">Text for OK button (default: "OK")</param>
    Task AlertAsync(string title, string message, string okButton = "OK");
}
