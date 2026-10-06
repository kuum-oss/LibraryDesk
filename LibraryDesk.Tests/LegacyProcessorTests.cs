using LibraryDesk.Core.Legacy;
using Xunit;

namespace LibraryDesk.Tests;

public sealed class LegacyProcessorTests
{
    [Fact]
    public void Process_RegularReader_Returns1292_70()
    {
        List<LoanLine> lines = new()
        {
            new LoanLine { Isbn = "A1", Days = 12, DailyRate = 100m },
            new LoanLine { Isbn = "B2", Days = 1, DailyRate = 250m },
        };

        decimal total = LegacyProcessor.Process(
            1, "Іваненко", "i@ex.com", true, lines,
            new DateOnly(2026, 3, 10), "", "Active", false, 60m,
            out string next);

        Assert.Equal(1292.70m, total);
        Assert.Equal("Returned", next);
    }

    [Fact]
    public void Process_SmallLoan_AddsDelivery()
    {
        List<LoanLine> lines = new()
        {
            new LoanLine { Isbn = "A1", Days = 2, DailyRate = 200m },
        };

        decimal total = LegacyProcessor.Process(
            2, "Петренко", "p@ex.com", false, lines,
            new DateOnly(2026, 3, 10), "SALE10", "Returned", false, 60m,
            out string next);

        Assert.Equal(420m, total);
        Assert.Equal("Overdue", next);
    }

    [Fact]
    public void Process_NoBooks_ReturnsMinusOne()
    {
        decimal total = LegacyProcessor.Process(
            3, "Коваль", "k@ex.com", false, new List<LoanLine>(),
            new DateOnly(2026, 3, 10), "", "Active", false, 60m,
            out _);

        Assert.Equal(-1m, total);
    }
}
