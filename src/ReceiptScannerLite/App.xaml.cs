namespace ReceiptScannerLite;

// MAUI App entry point - base class defined in App.xaml
public partial class App
{
    public App()
    {
        InitializeComponent();

        MainPage = new AppShell();
    }
}
