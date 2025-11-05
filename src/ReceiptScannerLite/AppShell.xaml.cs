namespace ReceiptScannerLite;

public partial class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();

        // Register routes for navigation
        Routing.RegisterRoute("receipt-detail", typeof(MainPage));
        Routing.RegisterRoute("review", typeof(MainPage));
    }
}
