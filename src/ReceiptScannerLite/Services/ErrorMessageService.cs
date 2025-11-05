using SQLite;
using System.IO;

namespace ReceiptScannerLite.Services;

public class ErrorMessageService : IErrorMessageService
{
    public string GetFriendlyMessage(Exception exception, string context)
    {
        return exception switch
        {
            SQLiteException sqlEx => GetSqliteErrorMessage(sqlEx, context),
            IOException ioEx => GetIoErrorMessage(ioEx, context),
            UnauthorizedAccessException => $"Access denied while {context}. Please check app permissions.",
            PermissionException permEx => GetPermissionErrorMessage(permEx, context),
            TimeoutException => $"The operation timed out while {context}. Please try again.",
            OperationCanceledException => $"The operation was cancelled.",
            InvalidOperationException invEx => GetInvalidOperationMessage(invEx, context),
            ArgumentException argEx => GetArgumentErrorMessage(argEx, context),
            _ => $"An unexpected error occurred while {context}. Please try again."
        };
    }

    public string GetSaveErrorMessage(Exception exception)
    {
        return exception switch
        {
            SQLiteException sqlEx when sqlEx.Message.Contains("constraint") =>
                "Unable to save: The receipt data violates database constraints. Please check your input.",
            SQLiteException sqlEx when sqlEx.Message.Contains("locked") =>
                "Unable to save: The database is currently locked. Please try again in a moment.",
            SQLiteException =>
                "Unable to save the receipt due to a database error. Please try again.",
            IOException when exception.Message.Contains("disk") || exception.Message.Contains("space") =>
                "Unable to save: Not enough storage space available. Please free up some space and try again.",
            IOException =>
                "Unable to save the receipt. Please check that the app has storage permissions and try again.",
            UnauthorizedAccessException =>
                "Unable to save: Access denied. Please check app permissions.",
            _ =>
                "Unable to save the receipt. Please try again, and contact support if the problem persists."
        };
    }

    public string GetDeleteErrorMessage(Exception exception)
    {
        return exception switch
        {
            SQLiteException sqlEx when sqlEx.Message.Contains("locked") =>
                "Unable to delete: The database is currently locked. Please try again in a moment.",
            SQLiteException =>
                "Unable to delete the receipt due to a database error. Please try again.",
            IOException =>
                "The receipt was deleted from the database, but its image file could not be removed. This may leave some unused files on your device.",
            UnauthorizedAccessException =>
                "Unable to delete: Access denied. Please check app permissions.",
            _ =>
                "Unable to delete the receipt. Please try again, and contact support if the problem persists."
        };
    }

    public string GetLoadErrorMessage(Exception exception)
    {
        return exception switch
        {
            SQLiteException sqlEx when sqlEx.Message.Contains("locked") =>
                "Unable to load: The database is currently locked. Please try again in a moment.",
            SQLiteException sqlEx when sqlEx.Message.Contains("corrupt") =>
                "Unable to load: The database may be corrupted. Please restart the app.",
            SQLiteException =>
                "Unable to load receipts due to a database error. Please try again.",
            _ =>
                "Unable to load receipts. Please try again, and contact support if the problem persists."
        };
    }

    public string GetImageErrorMessage(Exception exception)
    {
        return exception switch
        {
            FileNotFoundException =>
                "The image file could not be found. It may have been moved or deleted.",
            IOException when exception.Message.Contains("format") =>
                "The image format is not supported. Please use JPG or PNG images.",
            IOException when exception.Message.Contains("corrupt") =>
                "The image file appears to be corrupted and cannot be opened.",
            IOException =>
                "Unable to load the image. Please check that the file exists and the app has storage permissions.",
            UnauthorizedAccessException =>
                "Unable to access the image: Access denied. Please check app permissions.",
            OutOfMemoryException =>
                "The image is too large to load. Please try using a smaller image.",
            _ =>
                "Unable to load the image. Please try again."
        };
    }

    public string GetOcrErrorMessage(Exception exception)
    {
        return exception switch
        {
            FileNotFoundException when exception.Message.Contains("traineddata") =>
                "OCR data files are missing. Please reinstall the app to restore OCR functionality.",
            InvalidOperationException when exception.Message.Contains("preprocess") =>
                "Unable to process the image for OCR. Please try a different image with better lighting and clarity.",
            InvalidOperationException when exception.Message.Contains("Tesseract") =>
                "OCR engine is not available. The app will continue to work with manual entry.",
            IOException =>
                "Unable to process the image. Please check that the file is accessible and try again.",
            OutOfMemoryException =>
                "The image is too large to process. Please try using a smaller image or cropping it.",
            _ =>
                "Unable to perform OCR on the image. You can still enter receipt details manually."
        };
    }

    private string GetSqliteErrorMessage(SQLiteException sqlEx, string context)
    {
        if (sqlEx.Message.Contains("constraint"))
        {
            return $"Unable to complete {context}: Data validation failed. Please check your input.";
        }
        else if (sqlEx.Message.Contains("locked"))
        {
            return $"Unable to complete {context}: Database is busy. Please try again in a moment.";
        }
        else if (sqlEx.Message.Contains("corrupt"))
        {
            return $"Unable to complete {context}: Database may be corrupted. Please restart the app.";
        }
        else
        {
            return $"A database error occurred while {context}. Please try again.";
        }
    }

    private string GetIoErrorMessage(IOException ioEx, string context)
    {
        if (ioEx.Message.Contains("disk") || ioEx.Message.Contains("space"))
        {
            return $"Unable to complete {context}: Not enough storage space. Please free up some space.";
        }
        else if (ioEx.Message.Contains("find") || ioEx is FileNotFoundException)
        {
            return $"Unable to complete {context}: File not found.";
        }
        else
        {
            return $"A file system error occurred while {context}. Please check app permissions and try again.";
        }
    }

    private string GetPermissionErrorMessage(PermissionException permEx, string context)
    {
        if (context.Contains("camera", StringComparison.OrdinalIgnoreCase))
        {
            return "Camera permission is required to capture receipt photos. Please grant camera permission in your device settings.";
        }
        else if (context.Contains("photo", StringComparison.OrdinalIgnoreCase) ||
                 context.Contains("gallery", StringComparison.OrdinalIgnoreCase))
        {
            return "Photos permission is required to select images from your gallery. Please grant photos permission in your device settings.";
        }
        else if (context.Contains("storage", StringComparison.OrdinalIgnoreCase))
        {
            return "Storage permission is required to save files. Please grant storage permission in your device settings.";
        }
        else
        {
            return $"Permission denied while {context}. Please check app permissions in your device settings.";
        }
    }

    private string GetInvalidOperationMessage(InvalidOperationException invEx, string context)
    {
        if (invEx.Message.Contains("preprocess"))
        {
            return "Unable to process the image. Please try a different image with better quality.";
        }
        else
        {
            return $"Unable to complete {context}. Please try again.";
        }
    }

    private string GetArgumentErrorMessage(ArgumentException argEx, string context)
    {
        if (argEx.ParamName != null)
        {
            return $"Invalid input: {argEx.ParamName}. Please check your input and try again.";
        }
        else
        {
            return $"Invalid input while {context}. Please check your input and try again.";
        }
    }
}
