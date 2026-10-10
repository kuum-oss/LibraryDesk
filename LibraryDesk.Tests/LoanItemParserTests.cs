using LibraryDesk.Core.Domain;
using LibraryDesk.Core.Parsing;
using Xunit;

namespace LibraryDesk.Tests;

public sealed class LoanItemParserTests
{
    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("978-966-00-0001-0;2")]
    [InlineData(";2;10.50")]
    [InlineData("978-966-00-0001-0;0;10.50")]
    [InlineData("978-966-00-0001-0;-2;10.50")]
    [InlineData("978-966-00-0001-0;366;10.50")]
    [InlineData("978-966-00-0001-0;2;-10.50")]
    [InlineData("978-966-00-0001-0;2;10,50")]
    public void TryParse_InvalidInput_ReturnsFalse(string raw)
    {
        bool parsed = LoanItemParser.TryParse(raw, out LoanItem? item);

        Assert.False(parsed);
        Assert.Null(item);
    }

    [Fact]
    public void TryParse_ValidInvariantInput_ReturnsLoanItem()
    {
        bool parsed = LoanItemParser.TryParse(
            "978-966-00-0001-0;14;12.50",
            out LoanItem? item);

        Assert.True(parsed);
        Assert.NotNull(item);
        Assert.Equal(14, item.Days);
        Assert.Equal(12.50m, item.DailyRate);
    }
}
