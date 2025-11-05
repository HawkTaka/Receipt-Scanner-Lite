using FluentAssertions;
using ReceiptScannerLite.Services;
using Xunit;

namespace ReceiptScannerLite.Tests;

public class ParseServiceTests
{
    private readonly ParseService _parseService;

    public ParseServiceTests()
    {
        _parseService = new ParseService();
    }

    [Fact]
    public void Parse_SimpleReceipt_ExtractsBasicFields()
    {
        // Arrange
        var rawText = @"
WALMART SUPERCENTER
123 Main St
City, State 12345

Date: 01/15/2024

GROCERIES
Milk                 3.99
Bread                2.49
Eggs                 4.99

SUBTOTAL            11.47
TAX                  0.92
TOTAL               12.39
";

        // Act
        var result = _parseService.Parse(rawText);

        // Assert
        result.StoreName.Should().Be("WALMART SUPERCENTER");
        result.Date.Should().NotBeNull();
        result.Date!.Value.Year.Should().Be(2024);
        result.Date!.Value.Month.Should().Be(1);
        result.Date!.Value.Day.Should().Be(15);
        result.Subtotal.Should().Be(11.47m);
        result.Tax.Should().Be(0.92m);
        result.Total.Should().Be(12.39m);
        result.Items.Should().HaveCount(3);
    }

    [Fact]
    public void Parse_TotalOnly_ExtractsTotal()
    {
        // Arrange
        var rawText = @"
Store Name

Some items here

TOTAL: 45.67
";

        // Act
        var result = _parseService.Parse(rawText);

        // Assert
        result.Total.Should().Be(45.67m);
        result.StoreName.Should().Be("Store Name");
    }

    [Fact]
    public void Parse_NoTotal_FindsLargestAmount()
    {
        // Arrange
        var rawText = @"
RECEIPT

Item 1    5.99
Item 2    7.99
Item 3    45.67

Thank you!
";

        // Act
        var result = _parseService.Parse(rawText);

        // Assert
        result.Total.Should().Be(45.67m);
    }

    [Fact]
    public void Parse_MultipleFormats_ExtractsLineItems()
    {
        // Arrange
        var rawText = @"
Store

Coffee 2 x 3.50    7.00
Tea                5.99
Juice 1x4.50       4.50

TOTAL             17.49
";

        // Act
        var result = _parseService.Parse(rawText);

        // Assert
        result.Items.Should().HaveCount(3);
        result.Items[0].Description.Should().Be("Coffee 2 x 3.50");
        result.Items[0].LineTotal.Should().Be(7.00m);
    }

    [Fact]
    public void Parse_DifferentDateFormats_ExtractsDates()
    {
        // Test various date formats
        var testCases = new[]
        {
            ("Date: 2024-01-15", new DateTime(2024, 1, 15)),
            ("15/01/2024", new DateTime(2024, 1, 15)),
            ("01/15/2024", new DateTime(2024, 1, 15)),
            ("15.01.2024", new DateTime(2024, 1, 15)),
        };

        foreach (var (text, expectedDate) in testCases)
        {
            var result = _parseService.Parse(text);
            result.Date.Should().NotBeNull();
            result.Date!.Value.Date.Should().Be(expectedDate.Date);
        }
    }

    [Fact]
    public void Parse_TaxKeywords_ExtractsTax()
    {
        var testCases = new[]
        {
            "TAX: 5.50",
            "VAT: 5.50",
            "GST: 5.50"
        };

        foreach (var text in testCases)
        {
            var result = _parseService.Parse(text);
            result.Tax.Should().Be(5.50m);
        }
    }

    [Fact]
    public void Parse_EmptyText_ReturnsEmptyResult()
    {
        // Arrange
        var rawText = "";

        // Act
        var result = _parseService.Parse(rawText);

        // Assert
        result.StoreName.Should().BeNull();
        result.Date.Should().BeNull();
        result.Total.Should().BeNull();
        result.Items.Should().BeEmpty();
    }
}
