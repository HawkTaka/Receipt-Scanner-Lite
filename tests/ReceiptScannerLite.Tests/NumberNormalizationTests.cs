using FluentAssertions;
using ReceiptScannerLite.Services;
using Xunit;

namespace ReceiptScannerLite.Tests;

public class NumberNormalizationTests
{
    private readonly ParseService _parseService;

    public NumberNormalizationTests()
    {
        _parseService = new ParseService();
    }

    [Theory]
    [InlineData("12.34", 12.34)]
    [InlineData("12,34", 12.34)]
    [InlineData("1234.56", 1234.56)]
    [InlineData("1,234.56", 1234.56)]
    [InlineData("1 234,56", 1234.56)]
    [InlineData("1.234,56", 1234.56)]
    [InlineData("12 345.67", 12345.67)]
    [InlineData("0.99", 0.99)]
    [InlineData("0,99", 0.99)]
    [InlineData("999", 999)]
    [InlineData("1,000", 1000)]
    [InlineData("1.000", 1000)]
    public void TryNormalizeMoney_VariousFormats_ParsesCorrectly(string input, decimal expected)
    {
        // Act
        var success = _parseService.TryNormalizeMoney(input, out var result);

        // Assert
        success.Should().BeTrue();
        result.Should().Be(expected);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("abc")]
    [InlineData("12.34.56")]
    [InlineData("12,34,56")]
    public void TryNormalizeMoney_InvalidInput_ReturnsFalse(string input)
    {
        // Act
        var success = _parseService.TryNormalizeMoney(input, out var result);

        // Assert
        success.Should().BeFalse();
        result.Should().Be(0);
    }

    [Fact]
    public void TryNormalizeMoney_EuropeanFormat_ParsesCorrectly()
    {
        // European format: 1.234,56 (period as thousands, comma as decimal)
        var input = "1.234,56";
        var success = _parseService.TryNormalizeMoney(input, out var result);

        success.Should().BeTrue();
        result.Should().Be(1234.56m);
    }

    [Fact]
    public void TryNormalizeMoney_USFormat_ParsesCorrectly()
    {
        // US format: 1,234.56 (comma as thousands, period as decimal)
        var input = "1,234.56";
        var success = _parseService.TryNormalizeMoney(input, out var result);

        success.Should().BeTrue();
        result.Should().Be(1234.56m);
    }

    [Fact]
    public void TryNormalizeMoney_SpaceAsThousandsSeparator_ParsesCorrectly()
    {
        // Some countries use space as thousands separator
        var input = "1 234 567,89";
        var success = _parseService.TryNormalizeMoney(input, out var result);

        success.Should().BeTrue();
        result.Should().Be(1234567.89m);
    }

    [Theory]
    [InlineData("12.99", 12.99)]  // Ambiguous, but near end, treat as decimal
    [InlineData("12,99", 12.99)]  // Ambiguous, but near end, treat as decimal
    public void TryNormalizeMoney_AmbiguousSingleSeparator_TreatsAsDecimal(string input, decimal expected)
    {
        var success = _parseService.TryNormalizeMoney(input, out var result);

        success.Should().BeTrue();
        result.Should().Be(expected);
    }
}
