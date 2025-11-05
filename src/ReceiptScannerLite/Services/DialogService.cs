namespace ReceiptScannerLite.Services;

public class DialogService : IDialogService
{
    public async Task<bool> ConfirmAsync(string title, string message, string accept = "Yes", string cancel = "No")
    {
        if (Application.Current?.MainPage == null)
        {
            // If no main page available, default to false (don't perform destructive action)
            return false;
        }

        return await Application.Current.MainPage.DisplayAlert(title, message, accept, cancel);
    }

    public async Task AlertAsync(string title, string message, string okButton = "OK")
    {
        if (Application.Current?.MainPage == null)
        {
            // If no main page available, silently return
            return;
        }

        await Application.Current.MainPage.DisplayAlert(title, message, okButton);
    }
}
