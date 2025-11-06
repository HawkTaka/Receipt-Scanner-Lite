# Receipt Scanner Lite

A small, offline-first receipt scanner application built with .NET MAUI and Blazor. Capture receipt photos, run on-device OCR using Tesseract, parse key fields, and manage your receipts locally with SQLite.

![.NET MAUI](https://img.shields.io/badge/.NET-MAUI-512BD4?style=flat-square)
![Platform](https://img.shields.io/badge/Platform-Android%20%7C%20Windows-green?style=flat-square)
![License](https://img.shields.io/badge/License-MIT-blue?style=flat-square)
![CI Build](https://github.com/HawkTaka/Receipt-Scanner-Lite/workflows/CI%20Build%20and%20Test/badge.svg)
![Release](https://github.com/HawkTaka/Receipt-Scanner-Lite/workflows/Release%20Build/badge.svg)

## Features

### Core Functionality
- 📸 **Capture or Import** receipt images from camera or gallery
- 🔍 **On-Device OCR** using Tesseract (no cloud, no API keys required)
- 🖼️ **Image Preprocessing** with SkiaSharp for better OCR accuracy
- 🧠 **Heuristic Parsing** to extract Date, Total, Tax/VAT, Store name, and Line items
- 💾 **Local Storage** with SQLite - all data stays on your device
- ✏️ **Review & Edit** parsed data before saving
- 🔍 **Search & Filter** receipts by date, category, or store
- 📊 **Insights** with monthly totals and category breakdowns
- 📤 **CSV Export** for both receipts and line items

### Privacy & Offline First
- ✅ No cloud services or external APIs
- ✅ No user accounts or authentication
- ✅ All processing happens on-device
- ✅ Complete offline functionality after initial setup

## Screenshots

*(Add screenshots here when available)*

## Architecture

### Tech Stack
- **Framework**: .NET 8, MAUI Blazor Hybrid
- **UI**: Razor components (Blazor) inside MAUI Shell
- **Database**: SQLite via `sqlite-net-pcl`
- **State Management**: CommunityToolkit.Mvvm
- **OCR**: Tesseract with `eng.traineddata`
- **Image Processing**: SkiaSharp
- **Platforms**: Android (primary), Windows (secondary)

### Project Structure

```
ReceiptScannerLite/
├── src/ReceiptScannerLite/
│   ├── Data/
│   │   ├── Models/          # Receipt, LineItem entities
│   │   ├── Repositories/    # Data access layer
│   │   └── AppDb.cs         # SQLite connection & initialization
│   ├── Services/
│   │   ├── BootstrapService.cs       # First-run initialization
│   │   ├── FileService.cs            # Image capture/import
│   │   ├── ImagePreprocessService.cs # Image enhancement for OCR
│   │   ├── OcrService.cs             # Tesseract integration
│   │   ├── ParseService.cs           # Heuristic receipt parsing
│   │   └── CsvExportService.cs       # CSV generation
│   ├── ViewModels/          # MVVM ViewModels for each page
│   ├── Pages/               # Blazor components/pages
│   │   ├── Capture.razor
│   │   ├── Review.razor
│   │   ├── Receipts.razor
│   │   ├── ReceiptDetail.razor
│   │   ├── Insights.razor
│   │   └── Export.razor
│   ├── Assets/tessdata/     # Tesseract training data
│   └── MauiProgram.cs       # DI configuration
└── tests/ReceiptScannerLite.Tests/
    ├── ParseServiceTests.cs
    ├── NumberNormalizationTests.cs
    └── DateParsingTests.cs
```

## CI/CD & Automated Builds

This project uses GitHub Actions for continuous integration and automated releases.

### Build Status

- **CI Pipeline**: Runs on every push and PR to validate code changes
  - Builds for Android and Windows
  - Runs all tests with coverage reporting
  - Performs code quality checks

- **Release Pipeline**: Creates versioned releases automatically
  - Triggered by version tags (e.g., `v1.0.0`)
  - Produces production Android APK and Windows MSIX
  - Publishes to GitHub Releases

### Download Latest Release

Go to [Releases](https://github.com/HawkTaka/Receipt-Scanner-Lite/releases) to download:
- **Android**: `ReceiptScannerLite-vX.X.X-android.apk`
- **Windows**: `ReceiptScannerLite-vX.X.X-windows.msix`

See [CI/CD Documentation](docs/CI-CD.md) for complete details on the build pipeline.

## Getting Started

### Prerequisites

- **.NET 8 SDK** or later ([Download](https://dotnet.microsoft.com/download/dotnet/8.0))
- **Visual Studio 2022** (v17.8+) with MAUI workload, or
- **Visual Studio Code** with C# Dev Kit extension
- **Android SDK** (for Android builds)
- **Tesseract Training Data**: Download `eng.traineddata` from [tessdata](https://github.com/tesseract-ocr/tessdata)

### Installation

1. **Clone the repository**
   ```bash
   git clone https://github.com/yourusername/receipt-scanner-lite.git
   cd receipt-scanner-lite
   ```

2. **Add Tesseract Training Data**

   Download `eng.traineddata` and place it in:
   ```
   src/ReceiptScannerLite/Assets/tessdata/eng.traineddata
   ```

   You can download it from:
   - [tessdata (GitHub)](https://github.com/tesseract-ocr/tessdata/raw/main/eng.traineddata)

3. **Restore NuGet packages**
   ```bash
   dotnet restore
   ```

4. **Build the solution**
   ```bash
   dotnet build
   ```

### Running the App

#### Android
```bash
# Using dotnet CLI
dotnet build -t:Run -f net8.0-android

# Or in Visual Studio
# Set ReceiptScannerLite as startup project
# Select Android Emulator or device
# Press F5
```

#### Windows
```bash
# Using dotnet CLI
dotnet build -t:Run -f net8.0-windows10.0.19041.0

# Or in Visual Studio
# Set ReceiptScannerLite as startup project
# Select Windows Machine
# Press F5
```

### Running Tests
```bash
cd tests/ReceiptScannerLite.Tests
dotnet test
```

## Usage Guide

### 1. Capture a Receipt
- Open the app and tap **Capture** tab
- Choose **Take Photo** (camera) or **Pick from Gallery**
- Once captured, tap **Run OCR** to process

### 2. Review & Edit
- The app will extract receipt details automatically
- Review and edit:
  - Store name
  - Date
  - Subtotal, Tax, Total
  - Category
  - Line items
- Tap **Save Receipt** when done

### 3. Browse Receipts
- Tap **Receipts** tab to see all saved receipts
- Use filters to search by:
  - Store name
  - Date range
  - Category
- Tap any receipt to view details

### 4. View Insights
- Tap **Insights** tab
- See monthly spending totals
- View category breakdowns with percentages

### 5. Export Data
- Tap **Export** tab
- Optionally select a date range
- Export to CSV:
  - **Receipts CSV**: All receipt data
  - **Line Items CSV**: Detailed line item data with receipt context

## How It Works

### Image Preprocessing Pipeline
1. **Scale**: Resize to max 2000px long edge (keeps detail, reduces memory)
2. **Grayscale**: Convert to single channel
3. **Contrast**: Boost contrast by 1.2x
4. **Adaptive Threshold**: Binarize using local mean (improves OCR accuracy)
5. **Save**: Temporary PNG for OCR processing

### OCR Process
- Uses **Tesseract 5** in LSTM mode
- Processes preprocessed images
- Returns raw text for parsing

### Parsing Strategy
The ParseService uses heuristic rules to extract:

- **Store Name**: First capitalized/uppercase line (not a date/address)
- **Date**: Regex patterns for common formats (YYYY-MM-DD, DD/MM/YYYY, MM/DD/YYYY)
- **Total**: Line with "TOTAL" keyword + amount, or largest amount in bottom third
- **Tax/VAT**: Line with "TAX"/"VAT"/"GST" keyword + amount
- **Subtotal**: Line with "SUBTOTAL" keyword + amount
- **Line Items**: Lines with amounts, excluding keyword lines

### Number Normalization
Supports multiple international formats:
- `1,234.56` (US: comma thousands, period decimal)
- `1.234,56` (EU: period thousands, comma decimal)
- `1 234,56` (Space thousands, comma decimal)
- Automatic detection based on separator positions

## Configuration

### Categories
Default categories are defined in `ReviewViewModel.cs`:
- Uncategorized
- Groceries
- Dining
- Transportation
- Entertainment
- Shopping
- Healthcare
- Utilities
- Other

To customize, edit the `Categories` property in ViewModels.

### Database Location
- **Android**: `/data/data/com.receiptscannerlite.app/files/receipts.db`
- **Windows**: `%LOCALAPPDATA%\ReceiptScannerLite\receipts.db`

### Export Location
CSV files are saved to the app's data directory:
- Check **Export** page for the exact path on your device

## Troubleshooting

### OCR Not Working
1. Verify `eng.traineddata` is in `Assets/tessdata/`
2. Check console logs for initialization errors
3. Ensure the file is marked as `AndroidAsset` (Android) or copied on build (Windows)

### Poor OCR Accuracy
- Ensure good lighting when capturing
- Keep receipt flat and in focus
- Try cropping to just the receipt area
- Clean the camera lens

### App Crashes on Startup
- Check that SQLite initialization succeeded
- Verify all NuGet packages are restored
- Check Android SDK and emulator versions

### Images Not Saving
- Verify camera and storage permissions (Android)
- Check available storage space

## Testing

The project includes comprehensive unit tests for:

- **ParseService**: Receipt parsing logic
- **Number Normalization**: International number format handling
- **Date Parsing**: Multiple date format support

Run tests:
```bash
dotnet test
```

With coverage:
```bash
dotnet test /p:CollectCoverage=true
```

## Roadmap / Stretch Goals

- [ ] Perspective crop UI before OCR
- [ ] Bulk import from folder
- [ ] Multiple OCR languages (e.g., `afr`, `spa`, `fra`)
- [ ] Duplicate detection by hash
- [ ] Dark mode support
- [ ] Receipt statistics charts
- [ ] Cloud backup (optional, user-controlled)
- [ ] iOS platform support

## Contributing

Contributions are welcome! Please:

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/amazing-feature`)
3. Commit your changes (`git commit -m 'Add amazing feature'`)
4. Push to the branch (`git push origin feature/amazing-feature`)
5. Open a Pull Request

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## Acknowledgments

- [Tesseract OCR](https://github.com/tesseract-ocr/tesseract) - Open source OCR engine
- [.NET MAUI](https://dotnet.microsoft.com/apps/maui) - Cross-platform framework
- [SkiaSharp](https://github.com/mono/SkiaSharp) - 2D graphics library
- [SQLite](https://www.sqlite.org/) - Local database

## Support

For issues, questions, or suggestions:
- Open an [Issue](https://github.com/yourusername/receipt-scanner-lite/issues)
- Check existing issues first
- Provide detailed information (device, OS version, logs)

---

**Built with ❤️ using .NET MAUI**
