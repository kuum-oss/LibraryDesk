using LibraryDesk.Core.Domain;
using LibraryDesk.Core.Documents;
using Xunit;

namespace LibraryDesk.Tests;

public sealed class DocumentCalculatorTests
{
    [Fact]
    public void Calculate_RegularReader_Returns1292_70()
    {
        LoanCalculationRequest request = new(
            1, "Іваненко", "i@ex.com", true,
            new List<LoanItem>
            {
                new() { Isbn = "A1", Days = 12, DailyRate = 100m },
                new() { Isbn = "B2", Days = 1, DailyRate = 250m },
            },
            new DateOnly(2026, 3, 10), null, LoanStatus.Active, 60m);

        LoanTotalResult result = DocumentTotalCalculator.Calculate(request);

        Assert.Equal(1292.70m, result.Total);
        Assert.Equal(LoanStatus.Returned, result.NextStatus);
    }

    [Fact]
    public void Calculate_SmallLoan_AddsDelivery()
    {
        LoanCalculationRequest request = new(
            2, "Петренко", "p@ex.com", false,
            new List<LoanItem> { new() { Isbn = "A1", Days = 2, DailyRate = 200m } },
            new DateOnly(2026, 3, 10), PricingRules.SaleCoupon, LoanStatus.Returned, 60m);

        LoanTotalResult result = DocumentTotalCalculator.Calculate(request);

        Assert.Equal(420m, result.Total);
        Assert.Equal(LoanStatus.Overdue, result.NextStatus);
    }

    [Fact]
    public void Calculate_NoBooks_ThrowsArgument()
    {
        LoanCalculationRequest request = CreateRequest(Array.Empty<LoanItem>());

        Assert.Throws<ArgumentException>(() => DocumentTotalCalculator.Calculate(request));
    }

    [Fact]
    public void Calculate_ZeroDays_ThrowsRange()
    {
        LoanItem item = new() { Isbn = "A1", Days = 0, DailyRate = 10m };

        Assert.Throws<ArgumentOutOfRangeException>(() => DocumentGuards.EnsureLineValid(item));
    }

    [Fact]
    public void Calculate_InvalidReaderEmail_ThrowsArgument()
    {
        LoanCalculationRequest request = CreateRequest(
            new[] { new LoanItem { Isbn = "A1", Days = 1, DailyRate = 10m } }) with
        { ReaderEmail = "invalid-email" };

        Assert.Throws<ArgumentException>(() => DocumentTotalCalculator.Calculate(request));
    }

    private static LoanCalculationRequest CreateRequest(IReadOnlyList<LoanItem> items)
        => new(1, "Читач", "reader@example.com", false, items,
            new DateOnly(2026, 3, 10), null, LoanStatus.Active, 60m);
}
