namespace ReceiptScannerLite.Services;

public interface ICategoryService
{
    /// <summary>
    /// Gets all available receipt categories.
    /// </summary>
    IReadOnlyList<string> GetAllCategories();

    /// <summary>
    /// Checks if a category is valid.
    /// </summary>
    bool IsValidCategory(string category);

    /// <summary>
    /// Gets the default category for new receipts.
    /// </summary>
    string GetDefaultCategory();
}

public class CategoryService : ICategoryService
{
    private static readonly List<string> _categories = new()
    {
        "Uncategorized",
        "Groceries",
        "Dining",
        "Transportation",
        "Entertainment",
        "Shopping",
        "Healthcare",
        "Utilities",
        "Other"
    };

    private static readonly HashSet<string> _categorySet = new(_categories);

    public IReadOnlyList<string> GetAllCategories()
    {
        return _categories.AsReadOnly();
    }

    public bool IsValidCategory(string category)
    {
        return _categorySet.Contains(category);
    }

    public string GetDefaultCategory()
    {
        return "Uncategorized";
    }
}
