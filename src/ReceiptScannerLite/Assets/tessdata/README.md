# Tesseract Training Data

This directory should contain the Tesseract OCR training data file(s).

## Required File

You must download `eng.traineddata` and place it in this directory before building the app.

### Download Links

- **English (Best)**: [eng.traineddata](https://github.com/tesseract-ocr/tessdata_best/raw/main/eng.traineddata) (~10.6 MB)
- **English (Fast)**: [eng.traineddata](https://github.com/tesseract-ocr/tessdata_fast/raw/main/eng.traineddata) (~2.4 MB)
- **English (Standard)**: [eng.traineddata](https://github.com/tesseract-ocr/tessdata/raw/main/eng.traineddata) (~4.9 MB)

### Recommended

For best OCR accuracy on receipts, use the **Best** version.

### Installation

1. Download `eng.traineddata` from one of the links above
2. Place it in this directory: `src/ReceiptScannerLite/Assets/tessdata/eng.traineddata`
3. Rebuild the project

### File Structure

```
Assets/tessdata/
├── README.md (this file)
└── eng.traineddata (you need to download this)
```

### Additional Languages

If you want to support other languages:

1. Download additional `.traineddata` files from [tessdata](https://github.com/tesseract-ocr/tessdata)
2. Place them in this directory
3. Update `OcrService.cs` to use the appropriate language code

### Troubleshooting

If OCR fails to initialize:
- Verify `eng.traineddata` exists in this directory
- Check the file size (should be several MB)
- Ensure the file isn't corrupted (try re-downloading)
- Check the app logs for specific error messages
