using FluentAssertions;
using ReceiptScannerLite.Services;
using Xunit;

namespace ReceiptScannerLite.Tests;

public class DateParsingTests
{
    private readonly ParseService _parseService;

    public DateParsingTests()
    {
        _parseService = new ParseService();
    }

    [Theory]
    [InlineData("Date: 2024-01-15", 2024, 1, 15)]
    [InlineData("2024/01/15", 2024, 1, 15)]
    public void Parse_ISODateFormat_ExtractsDate(string text, int year, int month, int day)
    {
        // Act
        var result = _parseService.Parse(text);

        // Assert
        result.Date.Should().NotBeNull();
        result.Date!.Value.Year.Should().Be(year);
        result.Date!.Value.Month.Should().Be(month);
        result.Date!.Value.Day.Should().Be(day);
    }

    [Theory]
    [InlineData("15/01/2024", 2024, 1, 15)]
    [InlineData("15-01-2024", 2024, 1, 15)]
    [InlineData("15.01.2024", 2024, 1, 15)]
    public void Parse_DDMMYYYYFormat_ExtractsDate(string text, int year, int month, int day)
    {
        // Act
        var result = _parseService.Parse(text);

        // Assert
        result.Date.Should().NotBeNull();
        result.Date!.Value.Year.Should().Be(year);
        result.Date!.Value.Month.Should().Be(month);
        result.Date!.Value.Day.Should().Be(day);
    }

    [Theory]
    [InlineData("01/15/2024", 2024, 1, 15)]
    [InlineData("01-15-2024", 2024, 1, 15)]
    [InlineData("01.15.2024", 2024, 1, 15)]
    public void Parse_MMDDYYYYFormat_ExtractsDate(string text, int year, int month, int day)
    {
        // Act
        var result = _parseService.Parse(text);

        // Assert
        result.Date.Should().NotBeNull();
        result.Date!.Value.Year.Should().Be(year);
        result.Date!.Value.Month.Should().Be(month);
        result.Date!.Value.Day.Should().Be(day);
    }

    [Theory]
    [InlineData("15/01/24", 2024, 1, 15)]
    [InlineData("01/15/24", 2024, 1, 15)]
    public void Parse_TwoDigitYear_ExtractsDate(string text, int year, int month, int day)
    {
        // Act
        var result = _parseService.Parse(text);

        // Assert
        result.Date.Should().NotBeNull();
        result.Date!.Value.Year.Should().Be(year);
    }

    [Fact]
    public void Parse_NoDate_ReturnsNull()
    {
        // Arrange
        var text = @"
Store Name
Item 1  5.99
TOTAL  5.99
";

        // Act
        var result = _parseService.Parse(text);

        // Assert
        result.Date.Should().BeNull();
    }

    [Fact]
    public void Parse_MultipleValidDates_ExtractsFirst()
    {
        // Arrange
        var text = @"
Receipt from 2024-01-15
Expires: 2024-12-31
TOTAL: 10.00
";

        // Act
        var result = _parseService.Parse(text);

        // Assert
        result.Date.Should().NotBeNull();
        result.Date!.Value.Year.Should().Be(2024);
        result.Date!.Value.Month.Should().Be(1);
        result.Date!.Value.Day.Should().Be(15);
    }
}
